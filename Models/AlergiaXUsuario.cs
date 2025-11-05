public class AlergiaXUsuario{
    public int Id{get; set;}
    public int IdUsuario{get; set;}
    public int IdAlergia{get; set;}
    public bool Estado{get; set;}
    public string Motivo{get; set;}

    public void CambiarEstado(bool Activa)
    {
        Estado = !Activa;
    }

}