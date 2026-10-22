namespace back_end.src.Medicoes.Domain;

public class MedicoesEntity
{
    public int Id { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public back_end.src.Domain.Coleta.ColetaEntity? Coleta { get; private set; }
    public int? ColetaId { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public back_end.src.Domain.Qualidade.QualidadeEntity? Qualidade { get; private set; }
    public int? QualidadeId { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public back_end.src.Domain.QualidadeFutura.QualidadeFuturaEntity? QualidadeFutura { get; private set; }
    public int? QualidadeFuturaId { get; private set; }
    public Medicao codigoMedicao { get; private set; }
    public double? valor { get; private set; }
    public string unidade { get; private set; } = null!;
    public bool censurado { get; private set; }
    public double? limite { get; private set; }

    private MedicoesEntity() { }

    [System.Text.Json.Serialization.JsonConstructor]
    public MedicoesEntity(
        Medicao codigoMedicao,
        double? valor,
        string unidade,
        bool censurado,
        double? limite)
    {
        Atualizar(codigoMedicao, valor, unidade, censurado, limite);
    }

    public void VincularColeta(int coletaId)
    {
        if (coletaId <= 0)
            throw new ArgumentException("Id da coleta inválido.");
        if (TemOutroVinculo(ColetaId, coletaId, QualidadeId, QualidadeFuturaId))
            throw new InvalidOperationException("Não é permitido transferir a medição para outra coleta.");
        ColetaId = coletaId;
    }

    public void VincularQualidade(int qualidadeId)
    {
        if (qualidadeId <= 0)
            throw new ArgumentException("Id da qualidade inválido.");
        if (TemOutroVinculo(QualidadeId, qualidadeId, ColetaId, QualidadeFuturaId))
            throw new InvalidOperationException("A medição já está vinculada a outra entidade.");
        QualidadeId = qualidadeId;
    }

    public void VincularQualidadeFutura(int qualidadeFuturaId)
    {
        if (qualidadeFuturaId <= 0)
            throw new ArgumentException("Id da qualidade futura inválido.");
        if (TemOutroVinculo(QualidadeFuturaId, qualidadeFuturaId, ColetaId, QualidadeId))
            throw new InvalidOperationException("A medição já está vinculada a outra entidade.");
        QualidadeFuturaId = qualidadeFuturaId;
    }

    private static bool TemOutroVinculo(int? vinculoAtual, int novoId, int? outroVinculo1, int? outroVinculo2)
        => (vinculoAtual.HasValue && vinculoAtual.Value != novoId)
            || outroVinculo1.HasValue
            || outroVinculo2.HasValue;

    public void Atualizar(Medicao codigo, double? valor, string unidade, bool censurado, double? limite)
    {
        if (!Enum.IsDefined(codigo))
            throw new ArgumentException("Código de medição inválido.");
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Informe a unidade da medição.");
        if (valor is double v && (!double.IsFinite(v) || (codigo != Medicao.Temperatura && v < 0)))
            throw new ArgumentException("O valor deve ser finito e não negativo, exceto temperatura.");
        if (codigo == Medicao.Ph && valor > 14)
            throw new ArgumentException("O pH deve estar entre 0 e 14.");
        if (!censurado && valor is null)
            throw new ArgumentException("Informe o valor da medição não censurada.");
        if (limite is double l && (!double.IsFinite(l) || l <= 0))
            throw new ArgumentException("O limite deve ser finito e positivo.");
        if (censurado && limite is null)
            throw new ArgumentException("Informe o limite da medição censurada.");

        codigoMedicao = codigo;
        this.valor = valor;
        this.unidade = unidade.Trim();
        this.censurado = censurado;
        this.limite = limite;
    }
}
