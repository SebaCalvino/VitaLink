public class Alergia{
    public int Id{get; set;}
    public int IdUsuario{get; set;}
    public string Motivo{get; set;}
    public string Reaccion{get; set;}
    public string Severidad{get; set;}
    public bool Estado{get; set;}

    public void CambiarEstado(bool Activa)
    {
        Estado = Activa;
    }
}