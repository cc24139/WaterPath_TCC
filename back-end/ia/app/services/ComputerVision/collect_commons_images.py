"""Coleta candidatos reais com autoria e licença; não atribui classes automaticamente."""
import argparse
import hashlib
import html
import json
from pathlib import Path
import re
import time
import urllib.parse
import urllib.request
import urllib.error
from PIL import Image, ImageOps, ImageDraw

BASE = Path(__file__).resolve().parent
API = 'https://commons.wikimedia.org/w/api.php'
HEADERS = {'User-Agent': 'WaterPathTCC/1.0 (educational computer vision dataset; Wikimedia Commons research)'}
QUERIES = {
    'drainage_connection': [('intitle:outfall storm', 14), ('intitle:outfall sewer', 14), ('intitle:outfall drain', 10)],
    'urbano': [('"Rio Pinheiros"', 10), ('"Rio Tiete" "Sao Paulo"', 10), ('"Capibaribe" Recife', 10), ('"urban river"', 10)],
}

def fetch(url):
    for attempt in range(4):
        try:
            with urllib.request.urlopen(urllib.request.Request(url, headers=HEADERS), timeout=35) as response:
                return response.read()
        except urllib.error.HTTPError as error:
            if attempt == 3:
                raise
            delay = max(30, int(error.headers.get('Retry-After', '30'))) if error.code == 429 else 3
            print(f'Servidor solicitou pausa: {error.code}; aguardando {delay}s', flush=True)
            time.sleep(delay)
        except Exception:
            if attempt == 3:
                raise
            time.sleep(3 * (attempt + 1))

def api(**params):
    time.sleep(1.5)
    return json.loads(fetch(API + '?' + urllib.parse.urlencode({'action': 'query', 'format': 'json', **params})))

def plain(value):
    return html.unescape(re.sub('<[^>]+>', '', value)).strip()

def collect(destination):
    destination.mkdir(parents=True, exist_ok=True)
    metadata_path = destination / 'sources.json'
    rows = json.loads(metadata_path.read_text(encoding='utf-8')) if metadata_path.exists() else []
    seen = {r['title'] for r in rows}
    for candidate_class, queries in QUERIES.items():
        for query, limit in queries:
            results = api(list='search', srsearch=query, srnamespace=6, srlimit=limit)['query']['search']
            for result in results:
                title = result['title']
                if title in seen or not title.lower().endswith(('.jpg', '.jpeg', '.png')):
                    continue
                try:
                    page = next(iter(api(titles=title, prop='imageinfo', iiprop='url|size|extmetadata')['query']['pages'].values()))
                except Exception as error:
                    print(f'Metadados indisponíveis: {title}: {error}', flush=True)
                    continue
                info = page['imageinfo'][0]
                ext = info.get('extmetadata', {})
                license_name = plain(ext.get('LicenseShortName', {}).get('value', ''))
                if not (license_name.startswith('CC BY') or license_name in {'CC0', 'Public domain'}):
                    continue
                if min(info['width'], info['height']) < 300:
                    continue
                if info.get('size', 0) > 15_000_000:
                    continue
                identifier = str(page['pageid'])
                directory = destination / candidate_class
                directory.mkdir(exist_ok=True)
                filename = directory / f'commons_{identifier}.jpg'
                try:
                    # Evita pedir geração de thumbnails ao servidor. Os originais
                    # também preservam a resolução para a curadoria e exportação.
                    raw_url = info['url']
                    payload = fetch(raw_url)
                    filename.write_bytes(payload)
                    with Image.open(filename) as image:
                        image.verify()
                except Exception as error:
                    print(f'Falha: {title}: {error}', flush=True)
                    if filename.exists():
                        filename.unlink()
                    continue
                row = {
                    'id': identifier, 'title': title, 'candidate_class': candidate_class,
                    'path': filename.relative_to(BASE).as_posix(), 'source_url': info['descriptionurl'],
                    'original_url': info['url'], 'download_url': raw_url,
                    'author': plain(ext.get('Artist', {}).get('value', '')),
                    'license': license_name, 'license_url': ext.get('LicenseUrl', {}).get('value', ''),
                    'description': plain(ext.get('ImageDescription', {}).get('value', '')),
                    'sha256': hashlib.sha256(payload).hexdigest(), 'retrieved_on': '2026-10-04',
                }
                rows.append(row)
                seen.add(title)
                metadata_path.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
                print(f'{candidate_class}: {identifier} {title}', flush=True)
                time.sleep(1)
    return rows

def sheets(rows, destination):
    destination.mkdir(parents=True, exist_ok=True)
    for start in range(0, len(rows), 20):
        canvas = Image.new('RGB', (1800, 1400), 'white')
        draw = ImageDraw.Draw(canvas)
        for position, row in enumerate(rows[start:start+20]):
            x, y = position % 4 * 450, position // 4 * 280
            with Image.open(BASE / row['path']) as image:
                preview = ImageOps.contain(ImageOps.exif_transpose(image).convert('RGB'), (440, 240))
                canvas.paste(preview, (x + (440-preview.width)//2, y))
            draw.text((x+4, y+242), f"{start+position:03d} {row['id']} {row['candidate_class']}", fill='black')
            draw.text((x+4, y+260), row['title'][5:][:49], fill='black')
        canvas.save(destination / f'commons_review_{start//20+1}.jpg', quality=94)

def collect_curated(path, destination):
    """Baixa tamanhos padrão de páginas cuja licença foi conferida previamente."""
    destination.mkdir(parents=True, exist_ok=True)
    metadata_path = destination / 'sources.json'
    rows = json.loads(metadata_path.read_text(encoding='utf-8')) if metadata_path.exists() else []
    seen = {row['id'] for row in rows}
    for item in json.loads(path.read_text(encoding='utf-8')):
        if item['id'] in seen:
            continue
        name = item['title'][5:].replace(' ', '_')
        digest = hashlib.md5(name.encode('utf-8')).hexdigest()
        relative = digest[0] + '/' + digest[:2] + '/' + urllib.parse.quote(name)
        original = 'https://upload.wikimedia.org/wikipedia/commons/' + relative
        url = 'https://thumb.wikimedia.org/wikipedia/commons/thumb/' + relative + '/' + str(item['width']) + 'px-' + urllib.parse.quote(name)
        directory = destination / item['candidate_class']
        directory.mkdir(exist_ok=True)
        filename = directory / ('commons_' + item['id'] + '.jpg')
        if filename.exists():
            payload = filename.read_bytes()
        else:
            payload = fetch(url)
            filename.write_bytes(payload)
        with Image.open(filename) as image:
            image.verify()
        rows.append({**item, 'path': filename.relative_to(BASE).as_posix(),
            'source_url': 'https://commons.wikimedia.org/wiki/' + urllib.parse.quote(item['title'].replace(' ', '_')),
            'original_url': original, 'download_url': url,
            'sha256': hashlib.sha256(payload).hexdigest(), 'retrieved_on': '2026-10-04',
            'description': item['title'][5:], 'metadata_method': 'Licença e autoria conferidas na página pública do arquivo.'})
        metadata_path.write_text(json.dumps(rows, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        print(item['id'] + ' ' + item['title'], flush=True)
        time.sleep(1)
    return rows

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--review-dir', type=Path, required=True)
    parser.add_argument('--curated', type=Path, help='Lista de fontes com licença previamente conferida; evita busca em lote.')
    args = parser.parse_args()
    destination = BASE / 'dataset_multilabel' / 'candidates'
    rows = collect_curated(args.curated, destination) if args.curated else collect(destination)
    sheets(rows, args.review_dir)
    print(f'{len(rows)} candidatos coletados; precisam de revisão visual antes do treinamento.')
