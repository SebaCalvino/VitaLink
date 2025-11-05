using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;


public static class BD
{
    private static string _connectionString = @"Server=localhost;Database=BDVitalink;Integrated Security=True;TrustServerCertificate=True;";


    public static List<Usuario> ObtenerUsuarios()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Usuarios";
            return db.Query<Usuario>(sql).ToList();
        }
    }


    public static Usuario ObtenerUsuarioPorId(int id)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Usuarios WHERE Id = @pid";
            return db.QueryFirstOrDefault<Usuario>(sql, new { pid = id });
        }
    }


    public static List<Diagnostico> ObtenerDiagnosticosPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT D.*, P.Nombre AS NombrePatologia, P.Descripcion
                           FROM Diagnosticos D
                           INNER JOIN Patologias P ON D.IdPatologia = P.Id
                           WHERE D.IdUsuario = @idUsuario";
            return db.Query<Diagnostico>(sql, new { idUsuario }).ToList();
        }
    }


    public static List<Alergia> ObtenerAlergiasPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Alergias WHERE IdUsuario = @idUsuario";
            return db.Query<Alergia>(sql, new { idUsuario }).ToList();
        }
    }


    public static List<MedicacionesPaciente> ObtenerMedicacionesPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT M.*, R.NombreMedico, R.ApellidoMedico
                           FROM MedicacionesPaciente M
                           LEFT JOIN Recetas R ON M.IdReceta = R.Id
                           WHERE M.IdUsuario = @idUsuario";
            return db.Query<MedicacionPaciente>(sql, new { idUsuario }).ToList();
        }
    }


    public static List<VacunasXPaciente> ObtenerVacunasPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT VXP.*, V.NombreVacuna, V.Dosis, V.Aplicacion
                           FROM VacunasXPaciente VXP
                           INNER JOIN Vacunas V ON VXP.IdVacuna = V.Id
                           WHERE VXP.IdUsuario = @idUsuario";
            return db.Query<VacunaPaciente>(sql, new { idUsuario }).ToList();
        }
    }





    public static List<Organizacion> ObtenerOrganizaciones()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT O.*, D.Calle, D.Altura
                           FROM Organizaciones O
                           LEFT JOIN Direcciones D ON O.IdDireccion = D.Id";
            return db.Query<Organizacion>(sql).ToList();
        }
    }


}
