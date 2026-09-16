using back_end.src.Medicoes.Application;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using Application.DTOs.Coleta;
using System.Text.Json;
using back_end.src.Medicoes.Application.Handler;
using back_end.src.Medicoes.Controller;
using back_end.src.Medicoes.Domain;
using Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var assertions = 0;
void Check(bool value, string message)
{
    if (!value) throw new Exception(message);
    assertions++;
}
void Reject(Action action)
{
    try { action(); }
    catch (ArgumentException) { assertions++; return; }
    throw new Exception("Entrada inválida foi aceita.");
}

Reject(() => new MedicoesEntity((Medicao)999, 1, "mg/L", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, double.NaN, "pH", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, double.PositiveInfinity, "pH", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, -1, "pH", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, null, "pH", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, 7, " ", false, null));
Reject(() => new MedicoesEntity(Medicao.Ph, null, "pH", true, null));
Reject(() => new MedicoesEntity(Medicao.Ph, null, "pH", true, 0));
Reject(() => new MedicoesEntity(Medicao.Ph, null, "pH", true, double.NaN));
var entity = new MedicoesEntity(Medicao.Ph, 7, " pH ", false, null);
Check(entity.unidade == "pH", "Unidade não normalizada.");
Reject(() => entity.VincularColeta(0));
entity.VincularColeta(1);
try { entity.VincularColeta(2); throw new Exception("Transferência aceita."); }
catch (InvalidOperationException) { assertions++; }
entity.Atualizar(Medicao.Turbidez, null, "NTU", true, 0.1);
Check(entity.valor is null && entity.censurado && entity.limite == 0.1, "Atualização censurada incorreta.");
Reject(() => entity.Atualizar(Medicao.Ph, 7, "", false, null));
Check(entity.codigoMedicao == Medicao.Turbidez, "Atualização inválida alterou a entidade.");

var repository = new FakeRepository();
var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IMedicoesRepository>(repository);
services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(CreateMedicaoHandler).Assembly));
using var provider = services.BuildServiceProvider();
var controller = new MedicoesController(provider.GetRequiredService<IMediator>());
var token = CancellationToken.None;
Check(await controller.Cadastrar(new(1, new("Ph", 7, "pH")), token) is OkObjectResult, "Cadastro falhou.");
Check(await controller.ObterPorId(1, 1, token) is OkObjectResult { Value: MedicoesEntity }, "GET devolveu Task ou tipo incorreto.");
Check(await controller.ObterPorId(2, 1, token) is NotFoundObjectResult, "GET não respeita coleta.");
Check(await controller.ObterPorId(0, 1, token) is BadRequestObjectResult, "ID inválido não retorna 400.");
Check(await controller.Cadastrar(new(1, new("invalido", 7, "pH")), token) is BadRequestObjectResult, "Código inválido aceito.");
Check(await controller.Cadastrar(new(1, null!), token) is BadRequestObjectResult, "Payload nulo aceito.");
repository.Error = new KeyNotFoundException("Coleta não encontrada.");
Check(await controller.Cadastrar(new(9, new("Ph", 7, "pH")), token) is NotFoundObjectResult, "Coleta inexistente não retorna 404.");
repository.Error = new InvalidOperationException("Duplicada.");
Check(await controller.Cadastrar(new(1, new("Ph", 7, "pH")), token) is ConflictObjectResult, "Duplicidade não retorna 409.");
repository.Error = null;
Check(await controller.Atualizar(new(1, 1, new("Turbidez", 3, "NTU")), token) is OkObjectResult, "PUT falhou.");
Check(repository.Entity!.codigoMedicao == Medicao.Turbidez, "PUT não atualizou dados.");
Check(await controller.ObterTodos(token) is OkObjectResult { Value: List<MedicoesEntity> }, "Listagem falhou.");
Check(await controller.ObterPorColeta(1, token) is OkObjectResult { Value: List<MedicoesEntity> }, "Listagem por coleta falhou.");
Check(await controller.Deletar(1, 1, token) is NoContentResult, "DELETE falhou.");
Check(await controller.ObterPorId(1, 1, token) is NotFoundObjectResult, "DELETE não removeu.");
using var canceled = new CancellationTokenSource();
canceled.Cancel();
try { await controller.ObterTodos(canceled.Token); throw new Exception("Cancelamento ignorado."); }
catch (OperationCanceledException) { assertions++; }

using var context = new WaterPathDbContext(new DbContextOptionsBuilder<WaterPathDbContext>()
    .UseNpgsql("Host=localhost;Database=unused;Username=unused;Password=unused").Options);
var model = context.Model.FindEntityType(typeof(MedicoesEntity))!;
Check(model.GetTableName() == "Medicoes" && model.GetSchema() == "waterPath", "Tabela incorreta.");
Check(model.GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(new[] { "ColetaId", "codigoMedicao" })), "Índice único ausente.");
Check(model.GetForeignKeys().Single().DeleteBehavior == DeleteBehavior.Cascade, "FK/cascade incorreto.");
Check(model.FindProperty("valor")!.IsNullable && model.FindProperty("limite")!.IsNullable, "Censura exige colunas anuláveis.");
var sql = context.Database.GenerateCreateScript();
Check(sql.Contains("CREATE TABLE \"waterPath\".\"Medicoes\"") && sql.Contains("ON DELETE CASCADE"), "DDL inválido.");
var query = context.Medicoes.Where(m => m.ColetaId == 1 && m.Id == 2).ToQueryString();
Check(query.Contains("\"ColetaId\" = 1") && query.Contains("\"Id\" = 2"), "Consulta não traduzida.");
var coletaModel = context.Model.FindEntityType(typeof(ColetaEntity))!;
var corpoFk = coletaModel.GetForeignKeys().Single();
Check(corpoFk.PrincipalEntityType.ClrType == typeof(CorpoHidricoEntity)
    && corpoFk.Properties.Single().Name == "CorpoHidricoId" && corpoFk.IsRequired,
    "Coleta deve pertencer a um corpo hídrico obrigatório sem FK sombra.");
Check(model.GetForeignKeys().Single().PrincipalEntityType == coletaModel
    && model.GetForeignKeys().Single().IsRequired, "Medição deve pertencer a uma coleta.");
Reject(() => new ColetaEntity(0, DateTimeOffset.UtcNow, null, null, null));
var input = JsonSerializer.Deserialize<ColetaInput>("""
    {"corpoHidricoId":1,"dataHora":"2026-09-16T10:00:00-03:00",
     "medicoes":[{"codigoMedicao":"Ph","valor":7,"unidade":"pH"}]}
    """, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
var coleta = input.ToEntity();
Check(coleta.Medicoes.Count == 1 && coleta.DataHora.Offset == TimeSpan.Zero,
    "Payload deve aceitar medições e normalizar a data para UTC.");
context.Coletas.Add(coleta);
Check(ReferenceEquals(coleta.Medicoes[0].Coleta, coleta), "Navegação inversa não fixada pelo EF.");
Check(context.Entry(coleta.Medicoes[0]).Property(m => m.ColetaId).CurrentValue
    == context.Entry(coleta).Property(c => c.Id).CurrentValue, "FK temporária da medição incorreta.");
try { coleta.AdicionarMedicao(new MedicoesEntity(Medicao.Ph, 8, "pH", false, null)); throw new Exception("Duplicidade aceita."); }
catch (InvalidOperationException) { assertions++; }
var outraMedicao = new MedicoesEntity(Medicao.Turbidez, 3, "NTU", false, null);
outraMedicao.VincularColeta(999);
try { coleta.AdicionarMedicao(outraMedicao); throw new Exception("Medição de outra coleta aceita."); }
catch (InvalidOperationException) { assertions++; }
var coletaSql = context.Coletas.Include(c => c.Medicoes).ToQueryString();
Check(coletaSql.Contains("JOIN") && coletaSql.Contains("Medicoes"), "Consulta não carrega medições.");
Console.WriteLine($"{assertions} verificações passaram (domínio, handlers, controller e modelo PostgreSQL; sem conexão ao banco).");

sealed class FakeRepository : IMedicoesRepository
{
    public MedicoesEntity? Entity;
    public Exception? Error;
    public void Cadastrar(MedicoesEntity medicao, int coletaId)
    {
        if (Error is not null) throw Error;
        medicao.VincularColeta(coletaId);
        Entity = medicao;
    }
    public void Atualizar(MedicoesEntity medicao, int coletaId, int medicaoId) => Cadastrar(medicao, coletaId);
    public void Deletar(int coletaId, int medicaoId) => Entity = null;
    public MedicoesEntity? ObterPorId(int coletaId, int medicaoId) => Entity?.ColetaId == coletaId ? Entity : null;
    public List<MedicoesEntity> ObterPorColeta(int coletaId) => Entity?.ColetaId == coletaId ? [Entity] : [];
    public List<MedicoesEntity> ObterTodos() => Entity is null ? [] : [Entity];
}
