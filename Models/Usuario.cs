using System.Collections.Generic;
using System.Linq;

public class Usuario
{
    public int Id { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaCreacionCuenta { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public int Doc_nro { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public char Sexo { get; set; }
    public double PesoEnKg { get; set; }
    public double AlturaEnCm { get; set; }
    public int Telefono { get; set; }
    public string Email { get; set; }
    private string Contrasena;

    public int IntentosFallidos { get; private set; } = 0;
    private const int MaxIntentos = 5;

    public bool CambiarContraseña(string contrasenaActual, string contrasenaNueva)
    {
        if (IntentosFallidos >= MaxIntentos)
            return false;

        if (contrasenaActual == Contrasena)
        {
            Contrasena = contrasenaNueva;
            IntentosFallidos = 0; 
            return true;
        }
        else
        {
            IntentosFallidos++;
            return false;
        }
    }

    public void ResetearIntentos()
    {
        IntentosFallidos = 0;
    }
}