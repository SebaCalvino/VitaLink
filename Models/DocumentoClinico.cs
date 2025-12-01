public class DocumentoClinico{
    public int Id{get; set;}
    public int IdEncuentro{get; set;}
    public int Id_TipoDocumento{get; set;}
    public string Titulo{get; set;}
    public DateTime Fecha{get; set;}
    public int? IdArchivo{get; set;}
    public string NombreArchivo {get;set;}
    public string TipoArchivo {get;set;}
}
