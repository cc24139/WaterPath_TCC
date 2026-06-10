public class CadastrarImagemDTO
{
    public int IdCorpoHidrico { get; set; }
    public int? idColeta { get; set; }
    public DateTime DataUpload { get; set; }

    public required IFormFile Imagem { get; set; }
}