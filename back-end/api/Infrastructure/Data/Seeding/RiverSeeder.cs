using System.Text.Json;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.IA.Application;
using back_end.src.IA.Domain;
using back_end.src.Medicoes.Domain;
using Domain.User;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Seeding;

public sealed record SeedCounts(int Usuarios, int Rios, int Vinculos, int Coletas, int Medicoes, int Predicoes);
public sealed record SeedRiver(int Id, string Nome, int[] ColetaIds, int[] PredicaoIds);
public sealed record RiverSeedResult(string Dataset, SeedCounts Inseridos, SeedCounts TotaisDataset,
    int[] UsuarioIds, SeedRiver[] Rios);

public static class RiverSeeder
{
    public const string Dataset = "waterpath-sintetico-rios-v1";
    private const string Marker = "[SINTETICO v1]";
    private static readonly DateTimeOffset Start = new(2026, 9, 1, 12, 0, 0, TimeSpan.Zero);
    // PNG válido de 1x1 pixel: fixture, sem uso de modelos ou imagens reais.
    private static readonly byte[] Pixel = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+a5xkAAAAASUVORK5CYII=");
    private static readonly (string Name, string Place, double Size, double Lat, double Lon)[] Rivers =
    [
        ("Rio Aurora", "Vale fictício Norte / AM", 3.5, -3.1, -60.0),
        ("Rio das Pedras Azuis", "Serra fictícia / MG", 18, -19.9, -43.9),
        ("Rio Horizonte", "Planície fictícia / MS", 62.75, -20.4, -54.6),
        ("Rio Recorrência", "Bacia fictícia / SP", 140, -23.5, -46.6),
        ("Rio Fronteira", "Campos fictícios / RS", 480.25, -30.0, -51.2),
        ("Rio Sem Análise", "Sertão fictício / BA", 0.8, -12.9, -38.5)
    ];

    public static async Task<RiverSeedResult> SeedAsync(WaterPathDbContext db, string password,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Serializa execuções concorrentes desta rotina sem adicionar tabelas ou índices ao domínio.
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(871042026)", ct);
        var users = new List<UserEntity>();
        int nu = 0, nr = 0, nl = 0, nc = 0, nm = 0, np = 0;
        foreach (var index in new[] { 1, 2 })
        {
            var email = $"rios.sinteticos.{index}@waterpath.invalid";
            var name = $"{Marker} Pesquisador {index}";
            var user = await db.Usuarios.SingleOrDefaultAsync(u => u.Email == email, ct);
            if (user is null)
            {
                user = new UserEntity(name, BCrypt.Net.BCrypt.HashPassword(password), email);
                db.Usuarios.Add(user);
                nu++;
            }
            else if (user.Nome != name || !BCrypt.Net.BCrypt.Verify(password, user.Senha))
                throw new InvalidOperationException("Usuário da fixture já existe com identificação/senha diferente. Nenhum dado será sobrescrito.");
            users.Add(user);
        }
        await db.SaveChangesAsync(ct);
        var manifest = new List<SeedRiver>();
        for (var r = 0; r < Rivers.Length; r++)
        {
            var spec = Rivers[r];
            var name = $"{Marker} {spec.Name}";
            var place = $"{Marker} {spec.Place}; coordenadas fictícias";
            var river = await db.CorposHidricos.Include(x => x.users).SingleOrDefaultAsync(x => x.Nome == name, ct);
            if (river is null)
            {
                river = new CorpoHidricoEntity(name, place, spec.Size, false);
                db.CorposHidricos.Add(river);
                nr++;
            }
            else if (river.Localizacao != place || river.Tamanho != spec.Size)
                throw new InvalidOperationException($"Colisão com {name}: fixture alterada. Nenhum registro será sobrescrito.");
            foreach (var user in r % 2 == 0 ? users : users.Take(1))
                if (!river.users.Any(u => u.Id == user.Id)) { river.users.Add(user); nl++; }
            await db.SaveChangesAsync(ct);
            var collectionIds = new List<int>();
            var predictionIds = new List<int>();
            var previous = new List<object>();
            for (var day = 0; day < (r == 5 ? 1 : 6); day++)
            {
                var at = Start.AddDays(day);
                var oxygen = r == 2 ? 3.2 : r == 3 && day < 5 ? 4.4 : 6.5;
                var ph = r == 2 ? 5.7 : r == 4 ? 9.0 : 7.1;
                var temperature = 18.0 + r + day * 0.2;
                var conductivity = 90.0 + r * 40 + day;
                var collection = await db.Coletas.Include(c => c.Medicoes)
                    .SingleOrDefaultAsync(c => c.CorpoHidricoId == river.Id && c.DataHora == at, ct);
                if (collection is null)
                {
                    collection = new ColetaEntity(river.Id, at, spec.Lat, spec.Lon, 0.5 + r * 0.4);
                    db.Coletas.Add(collection);
                    nc++;
                }
                else if (collection.Latitude != spec.Lat || collection.Longitude != spec.Lon
                    || collection.ProfundidadeMetros != 0.5 + r * 0.4)
                    throw new InvalidOperationException("Coleta da fixture alterada; nenhum dado será sobrescrito.");
                var measures = new[]
                {
                    new MedicoesEntity(Medicao.Temperatura, temperature, "°C", false, null),
                    new MedicoesEntity(Medicao.Ph, ph, "pH", false, null),
                    new MedicoesEntity(Medicao.OxigenioDissolvido, oxygen, "mg/L", false, null),
                    new MedicoesEntity(Medicao.CondutividadeEletrica, conductivity, "µS/cm", false, null),
                    new MedicoesEntity(Medicao.SolidosSuspensosTotais, 12 + r * 5, "mg/L", false, null),
                    new MedicoesEntity(Medicao.CarbonoOrganicoTotal, 2 + r * 0.3, "mg/L", false, null),
                    new MedicoesEntity(Medicao.FosforoTotal, 0.02 + r * 0.01, "mg/L", false, null),
                    new MedicoesEntity(Medicao.Nitrato, null, "mg/L", true, 0.05)
                };
                foreach (var m in measures)
                {
                    var existing = collection.Medicoes.SingleOrDefault(x => x.codigoMedicao == m.codigoMedicao);
                    if (existing is null) { collection.AdicionarMedicao(m); nm++; }
                    else if (existing.valor != m.valor || existing.unidade != m.unidade
                        || existing.censurado != m.censurado || existing.limite != m.limite)
                        throw new InvalidOperationException("Medição da fixture alterada; nenhum dado será sobrescrito.");
                }
                await db.SaveChangesAsync(ct);
                collectionIds.Add(collection.Id);
                if (r == 5) continue;
                var file = $"{Dataset}-r{r + 1}-d{day + 1}.png";
                var prediction = await db.PredicoesIA.SingleOrDefaultAsync(p => p.ColetaId == collection.Id && p.NomeArquivo == file, ct);
                var visual = r is 1 or 2;
                var basis = 1 + (visual ? 1 : 0) + (ph < 6 || ph > 9 || oxygen < 5 ? 1 : 0);
                var history = previous.TakeLast(5).ToArray();
                var alertCount = r is 1 or 2 or 3 ? history.Length : 0;
                var level = Math.Min(3, basis + (alertCount >= 3 ? 1 : 0));
                if (prediction is null)
                {
                    var sample = JsonSerializer.Serialize(new { temperatura = temperature, ph,
                        oxigenio_dissolvido = oxygen, condutividade_eletrica = conductivity,
                        estacao = $"{Marker} estação {r + 1}", data = at.ToString("O") });
                    var result = JsonSerializer.Serialize(new
                    {
                        synthetic = true, dataset = Dataset,
                        detections = visual ? new[] { new { classId = 0, className = "Lixo", count = 1, indicatesRisk = true } } : [],
                        totalObjects = visual ? 1 : 0, metalPredictions = Array.Empty<object>(),
                        visionModelVersion = "fixture-sintetica-sem-inferencia",
                        riskLevel = level, baseRiskLevel = basis,
                        riskLabel = level == 1 ? "baixo" : level == 2 ? "moderado" : "alto",
                        riskRuleVersion = PredicaoIAService.VersaoRegraRisco,
                        riskReasons = new[] { $"{Marker} Resultado fictício para testes; sem inferência de IA." },
                        riskInputs = new { ph, oxigenio_dissolvido = oxygen, visualClasses = visual ? new[] { "Lixo" } : [] },
                        history = new { evaluatedCollections = history.Length, alertCollections = alertCount,
                            adjustment = level - basis, samples = history },
                        annotatedImageContentType = "image/png", annotatedImage = Convert.ToBase64String(Pixel)
                    });
                    if (!ContratoRisco.Valido(sample, result))
                        throw new InvalidOperationException("Fixture incompatível com o contrato de risco atual.");
                    prediction = new PredicaoIAEntity { ColetaId = collection.Id, CorpoHidricoId = river.Id,
                        DataColeta = at.UtcDateTime, CriadaEm = at.AddMinutes(10).UtcDateTime,
                        Tipo = "integrada", EntradaJson = sample, ResultadoJson = result, NomeArquivo = file,
                        ContentTypeOriginal = "image/png", ImagemOriginal = Pixel,
                        ContentTypeResultado = "image/png", ImagemResultado = Pixel };
                    db.PredicoesIA.Add(prediction);
                    np++;
                    await db.SaveChangesAsync(ct);
                }
                else if (prediction.CorpoHidricoId != river.Id || prediction.DataColeta != at.UtcDateTime
                    || prediction.Tipo != "integrada" || !ContratoRisco.Valido(prediction.EntradaJson, prediction.ResultadoJson ?? ""))
                    throw new InvalidOperationException("Predição da fixture alterada; nenhum dado será sobrescrito.");
                using (var saved = JsonDocument.Parse(prediction.ResultadoJson!))
                {
                    var root = saved.RootElement;
                    if (!root.TryGetProperty("synthetic", out var synthetic) || synthetic.ValueKind != JsonValueKind.True
                        || !root.TryGetProperty("dataset", out var dataset) || dataset.GetString() != Dataset
                        || root.GetProperty("riskLevel").GetInt32() != level
                        || root.GetProperty("baseRiskLevel").GetInt32() != basis)
                        throw new InvalidOperationException("Identificação/cenário da predição sintética alterado; nenhum dado será sobrescrito.");
                }
                predictionIds.Add(prediction.Id);
                previous.Add(new { predictionId = prediction.Id, coletaId = collection.Id, baseRiskLevel = basis });
            }
            manifest.Add(new SeedRiver(river.Id, name, collectionIds.ToArray(), predictionIds.ToArray()));
        }
        await transaction.CommitAsync(ct);
        return new RiverSeedResult(Dataset, new(nu, nr, nl, nc, nm, np), new(2, 6, 9, 31, 248, 30),
            users.Select(u => u.Id).ToArray(), manifest.ToArray());
    }
}
