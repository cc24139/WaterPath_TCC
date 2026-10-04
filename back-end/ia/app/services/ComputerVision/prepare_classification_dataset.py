"""Exporta rótulos de presença para três classificadores YOLO sem recortar as fotos.

Rótulos vazios representam incerteza e são excluídos somente daquela tarefa.
O manifesto é a fonte revisável de classes, grupos de origem e divisão.
"""
import argparse
import csv
import hashlib
import json
from collections import Counter, defaultdict
from pathlib import Path
import random
import shutil

from PIL import Image, ImageOps

BASE = Path(__file__).resolve().parent
CLASSES = ('lixo', 'drainage_connection', 'urbano')
SPLITS = ('train', 'val', 'test')

def read_manifest(path):
    with path.open(encoding='utf-8-sig', newline='') as stream:
        rows = list(csv.DictReader(stream))
    seen = set()
    for row in rows:
        if row['id'] in seen:
            raise ValueError(f"ID repetido: {row['id']}")
        seen.add(row['id'])
        if row['split'] not in SPLITS or not row['group_id']:
            raise ValueError(f"Divisão/grupo inválido: {row['id']}")
        for label in CLASSES:
            if row[label] not in {'0', '1', ''}:
                raise ValueError(f"Rótulo inválido: {row['id']} / {label}")
        path = (BASE / row['source_path']).resolve()
        if not path.is_relative_to(BASE.resolve()) or not path.is_file():
            raise ValueError(f"Imagem inválida: {row['source_path']}")
        row['_path'] = path
    return rows

def group_splits(rows, seed=42):
    """Sugere splits por grupos inteiros com positivos/negativos em cada tarefa."""
    grouped = defaultdict(list)
    for row in rows:
        grouped[row['group_id']].append(row)
    group_ids = sorted(grouped)
    rng = random.Random(seed)
    best = None
    for _ in range(5000):
        assignment = {group: rng.choices(SPLITS, weights=(70, 15, 15))[0] for group in group_ids}
        assignment['existing_litter_session'] = 'train'
        assignment['sahadara_video'] = 'train'
        totals = Counter()
        counts = Counter()
        for group, members in grouped.items():
            split = assignment[group]
            totals[split] += len(members)
            for row in members:
                for label in CLASSES:
                    if row[label] in {'0', '1'} and row['status'] == 'reviewed':
                        counts[split, label, row[label]] += 1
        score = sum(abs(totals[s] / len(rows) - target) for s, target in zip(SPLITS, (.7, .15, .15)))
        # Os rótulos incertos não devem dominar a divisão das tarefas menores.
        # Aproxima 70/15/15 para positivos e negativos de cada alvo conhecido.
        for label in CLASSES:
            for value in ('0', '1'):
                total = sum(counts[split, label, value] for split in SPLITS)
                if total:
                    score += sum(abs(counts[split, label, value] / total - target)
                                 for split, target in zip(SPLITS, (.7, .15, .15)))
        score += sum(max(0, 3 - counts[s, c, value]) * 10 for s in SPLITS for c in CLASSES for value in ('0', '1'))
        if best is None or score < best[0]:
            best = score, assignment
    for row in rows:
        row['split'] = best[1][row['group_id']]
    return rows

def export_dataset(manifest, output):
    rows = read_manifest(manifest)
    if output.exists():
        raise ValueError('A pasta de saída já existe. Escolha uma nova versão para preservar o dataset anterior.')
    groups = defaultdict(set)
    hashes = defaultdict(set)
    pixels = defaultdict(set)
    for row in rows:
        groups[row['group_id']].add(row['split'])
        digest = hashlib.sha256(row['_path'].read_bytes()).hexdigest()
        hashes[digest].add(row['split'])
        with Image.open(row['_path']) as image:
            image = ImageOps.exif_transpose(image).convert('RGB')
            pixel_key = hashlib.sha256(str(image.size).encode() + image.tobytes()).hexdigest()
            pixels[pixel_key].add(row['split'])
    if any(len(splits) > 1 for mapping in (groups, hashes, pixels) for splits in mapping.values()):
        raise ValueError('Grupo ou imagem idêntica aparece em mais de uma divisão.')
    selected = [row for row in rows if row['status'] == 'reviewed']
    counts = {label: {split: {'ausente': 0, 'presente': 0} for split in SPLITS} for label in CLASSES}
    for row in selected:
        for label in CLASSES:
            value = row[label]
            if value != '':
                counts[label][row['split']]['presente' if value == '1' else 'ausente'] += 1
    if any(n == 0 for task in counts.values() for split in task.values() for n in split.values()):
        raise ValueError('Cada classe precisa de positivos e negativos em treino, validação e teste.')
    output.mkdir(parents=True)
    for label in CLASSES:
        for split in SPLITS:
            for state in ('ausente', 'presente'):
                (output / label / split / state).mkdir(parents=True)
    for row in selected:
        for label in CLASSES:
            if row[label] == '':
                continue
            state = 'presente' if row[label] == '1' else 'ausente'
            destination = output / label / row['split'] / state / (row['id'] + '.jpg')
            # Mantém toda a cena e limita apenas a resolução; nunca desenha contornos.
            with Image.open(row['_path']) as image:
                image = ImageOps.exif_transpose(image).convert('RGB')
                image.thumbnail((1600, 1600))
                image.save(destination, quality=95)
    shutil.copy2(manifest, output / 'labels.csv')
    report = {
        'manifest_images': len(rows), 'reviewed_images': len(selected),
        'excluded_images': len(rows)-len(selected), 'counts': counts,
        'group_counts': dict(Counter(next(iter(splits)) for splits in groups.values())),
        'leakage_checks': {'group_overlap': 0, 'file_hash_overlap': 0, 'decoded_pixel_overlap': 0},
        'unknown_labels': {c: sum(r[c] == '' for r in selected) for c in CLASSES},
        'notes': ['Rótulos resultam de revisão visual assistida, sem validação humana independente.',
                  'Imagens locais herdadas não têm licença de redistribuição comprovada.',
                  'A divisão mantém grupos de origem juntos; não garante independência de local para todas as fotos externas.',
                  'Não há modelo treinado neste passo.'],
    }
    (output / 'summary.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    return report

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest', type=Path, default=BASE / 'dataset_multilabel/labels.csv')
    parser.add_argument('--output', type=Path, default=BASE / 'dataset_multilabel/yolo_classification_v2')
    args = parser.parse_args()
    print(json.dumps(export_dataset(args.manifest, args.output), ensure_ascii=False, indent=2))
