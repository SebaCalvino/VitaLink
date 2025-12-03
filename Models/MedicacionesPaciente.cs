public class MedicacionesPaciente{
    public int Id{get; set;}
    public int IdReceta{get; set;}
    public int IdUsuario{get; set;}
    public string Nombre_Comercial{get; set;}
    public string Dosis{get; set;}
    public string Via{get; set;}
    public string Frecuencia{get; set;}
    public string Indicacion{get; set;}
    public DateTime? HoraProgramada{get; set;}
    public DateTime? FechaFabricacion{get; set;}
    public DateTime? FechaVencimiento{get; set;}
    public bool? Estado{get; set;}
}