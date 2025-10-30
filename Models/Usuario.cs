using System.Collections.Generic;
using System.Linq;

public class Usuario{
    public int Id{get; set;}
    public bool Estado{get; set;}
    public DateTime FechaCreacionCuenta{get; set;}
    public string Nombre{get; set;}
    public string Apellido{get; set;}
    public int Doc_nro{get; set;}
    public DateTime FechaNacimiento{get; set;}
    public char Sexo{get; set;}
    public double PesoEnKg{get; set;}
    public double AlturaEnCm{get; set;}
    public int Telefono{get; set;}
    public string Email{get; set;}
    public string Contrasena{get; private set;}
    
    public bool ModificarContrasena(){

    }
}