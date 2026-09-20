namespace back_end.src.Medicoes.Domain;

public class MedicoesEntity
{
    public int Id { get; private set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public back_end.src.Domain.Coleta.ColetaEntity Coleta { get; private set; } = null!;
    public int ColetaId { get; private set; }
    public Medicao codigoMedicao { get; private set; }
    public double? valor { get; private set; }
    public string unidade { get; private set; } = null!;
    public bool censurado { get; private set; }
    public double? limite { get; private set; }

    private MedicoesEntity() { }

    public MedicoesEntity(Medicao codigo, double? valor, string unidade, bool censurado, double? limite)
    {
        Atualizar(codigo, valor, unidade, censurado, limite);
    }

    public void VincularColeta(int coletaId)
    {
        if (coletaId <= 0)
            throw new ArgumentException("Id da coleta inválido.");
        if (ColetaId != 0 && ColetaId != coletaId)
            throw new InvalidOperationException("Não é permitido transferir a medição para outra coleta.");
        ColetaId = coletaId;
    }

    public void Atualizar(Medicao codigo, double? valor, string unidade, bool censurado, double? limite)
    {
        if (!Enum.IsDefined(codigo))
            throw new ArgumentException("Código de medição inválido.");
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Informe a unidade da medição.");
        if (valor is double v && (!double.IsFinite(v) || v < 0))
            throw new ArgumentException("O valor deve ser finito e não negativo.");
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
