
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;




public static class BD
{
    private static string _connectionString = @"Server=localhost;Database=BDVitalink;Integrated Security=True;TrustServerCertificate=True;";


public static Usuario LoginUsuario(string email, string contrasena)
{
    using (SqlConnection db = new SqlConnection(_connectionString))
    {
        string sql = "SELECT * FROM Usuarios WHERE Email = @email AND Contrasena = @contrasena";
        return db.QueryFirstOrDefault<Usuario>(sql, new { email, contrasena });
    }
}


    public static Usuario ObtenerUsuarioPorId(int Id){
        using(SqlConnection db = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios
                            WHERE Id = @pId";
            return db.QueryFirstOrDefault<Usuario>(query, new {pId = Id});
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
            return db.Query<MedicacionesPaciente>(sql, new { idUsuario }).ToList();
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
            return db.Query<VacunasXPaciente>(sql, new { idUsuario }).ToList();
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


    public static string ObtenerNombreOrganizacion(int idOrganizacion)
    {
        if (idOrganizacion <= 0)
            return null;


        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT Nombre FROM Organizaciones WHERE Id = @idOrganizacion";
            return db.QueryFirstOrDefault<string>(sql, new { idOrganizacion });
        }
    }


    public static string ObtenerNombreVacuna(int idVacuna)
    {
        if (idVacuna <= 0)
            return null;


        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT NombreVacuna FROM Vacunas WHERE Id = @idVacuna";
            return db.QueryFirstOrDefault<string>(sql, new { idVacuna });
        }
    }


    public static string ObtenerModalidadEstudio(int idModalidad)
    {
        if (idModalidad <= 0)
            return null;


        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT Nombre FROM Modalidad WHERE Id = @idModalidad";
            return db.QueryFirstOrDefault<string>(sql, new { idModalidad });
        }
    }
   
    public static List<Encuentro> ObtenerEncuentrosPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT E.*, O.Nombre AS NombreOrganizacion, O.Tipo
                           FROM Encuentros E
                           LEFT JOIN Organizaciones O ON E.IdOrganizacion = O.Id
                           WHERE E.IdUsuario = @idUsuario";
            return db.Query<Encuentro>(sql, new { idUsuario }).ToList();
        }
    }








    public static List<DocumentoClinico> ObtenerDocumentosPorEncuentro(int idEncuentro)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT DC.*, A.Capacidad, A.TipoArchivo, A.FechaCreacion
                           FROM Documentos_Clinicos DC
                           LEFT JOIN Archivos A ON DC.IdArchivo = A.Id
                           WHERE DC.IdEncuentro = @idEncuentro";
            return db.Query<DocumentoClinico>(sql, new { idEncuentro }).ToList();
        }
    }




    public static List<Imagenes_Estudio> ObtenerImagenesPorEncuentro(int idEncuentro)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Imagenes_Estudios WHERE IdEncuentro = @idEncuentro";
            return db.Query<Imagenes_Estudio>(sql, new { idEncuentro }).ToList();
        }
    }


     




}




