# Insomnia — somente risco da IA

Importe `insomnia-ia-risco.json` no Insomnia usando **Import → File**. A coleção contém 13 cenários de **POST /analyze**, o endpoint da IA que calcula risco e variação temporal de metais. Não inclui rotas da API principal, inferência somente visual, `/predict` ou endpoints de informações. Essas outras rotas não calculam o risco retornado por `/analyze`.

No ambiente da coleção:

- `base_url`: `http://localhost:8000` por padrão. Para outra instância da **IA**, troque esse endereço, sem barra final. Não use a porta 5189 da API principal.
- `image_path`: caminho absoluto de um JPEG/PNG de até 10 MB no seu computador. Alternativamente, selecione o arquivo diretamente no campo `image`, tipo **File**, de cada requisição.

Os corpos já estão preenchidos como **Multipart Form**:

| Campo | Tipo | Conteúdo |
| --- | --- | --- |
| `image` | File | Imagem selecionada por você |
| `data` | Text | JSON preenchido com as medições e, nos casos específicos, metais atuais e referência anterior |
| `history` | Text | `[]` ou lista preenchida com até cinco coletas anteriores |

Deixe o Insomnia definir `Content-Type` e o boundary do multipart. A IA atual não exige token nessas chamadas e não consulta o banco para interpretar o histórico. Os IDs e as medições dos exemplos são sintéticos. Os quatro campos obrigatórios da amostra são `temperatura`, `ph`, `condutividade_eletrica` e `oxigenio_dissolvido`.

Os exemplos cobrem medições sem alerta, pH/OD alterados, valores de fronteira, recorrência, aumento/redução/estabilidade de metais, comparação parcial, ausência de referência e entradas inválidas (422). Há asserções de status e do contrato de resposta em **Scripts → After-response**. O nível final depende também das detecções da imagem; as asserções calculam o nível esperado com base no sinal visual retornado. Concentrações observadas são comparadas separadamente das estimativas do modelo e não aumentam o nível de risco por si só.

Para iniciar a IA local, a partir da raiz, em um terminal com as dependências/modelos preparados:

```powershell
Set-Location back-end/ia/app
& ../../../.venv-training/Scripts/python.exe -m uvicorn main:app --host 127.0.0.1 --port 8000
```

Use o **Collection Runner** para rodar os cenários ou envie cada requisição individualmente. O caso de imagem ausente já omite `image` propositalmente. Modelos ausentes ou falhas de inferência podem causar 503, que não é tratado como sucesso nos testes.

Para regenerar o arquivo, a partir da raiz:

```powershell
node back-end/ia/testing/generate-insomnia-risk.cjs
```

Os bodies são baseados em `app/main.py`, `DTOs/Amostra.py`, `DTOs/MetaisPesados.py` e `services/risk/classification.py`. A validação local dos bodies e dos scripts não substitui importação/execução no runtime nativo do Insomnia.
