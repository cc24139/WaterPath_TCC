namespace back_end.src.Medicoes.Domain
{
    public class MedicoesEntity
    {
        public int Id { get; private set; }
        public Medicao codigoMedicao { get; private set; }
        public double? valor { get; set; }
        public string unidade { get; set; }
        public bool censurado { get; set; }
        public double? limite {get;set;}

        public MedicoesEntity() { }
        
        public MedicoesEntity(
            Medicao codigo,
            double? valor,
            string unidade,
            bool censurado,
            double? limite
        )
        {
            if (valor < 0)
            {
                throw new Exception("valor da medição não pode ser negativo!");
            }
            this.valor = valor;
            this.codigoMedicao = codigo;
            this.unidade = unidade;
            this.censurado = censurado;
            this.limite = limite;
        }
    }
}