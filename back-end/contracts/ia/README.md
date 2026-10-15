# Contrato interno da IA

`v1/openapi.json` é a fonte canônica do protocolo entre a API ASP.NET Core e o serviço FastAPI. O arquivo cobre somente `POST /analyze`: campos multipart, JSONs transportados como texto e resposta interna. Rotas públicas da API principal e artefatos de treinamento não fazem parte dele.

Ao alterar a integração:

1. atualize primeiro o OpenAPI e seus exemplos;
2. ajuste os DTOs/validadores dos dois serviços;
3. execute os testes de compatibilidade abaixo;
4. para uma quebra incompatível, crie `v2` em vez de reescrever silenciosamente o contrato implantado.

Na raiz do repositório:

```powershell
dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj --filter FullyQualifiedName~IaSharedContractTests
Set-Location back-end/ia/app
python -X utf8 -B -m unittest discover -s tests -p 'test_shared_contract.py'
```

O teste C# copia o mesmo arquivo para a saída da suíte e exercita `ContratoAmostra` e o round-trip do `IaClient`. O teste Python compara os modelos Pydantic e o multipart gerado pelo FastAPI, além de validar a resposta montada pelo serviço.
