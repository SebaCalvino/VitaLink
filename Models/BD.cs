
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;




public static class BD
{
    private static string _connectionString = @"Server=LocalHost;Database=BDVitalink;Integrated Security=True;TrustServerCertificate=True;";


public static Usuario LoginUsuario(string email, string contrasena)
{
    using (SqlConnection db = new SqlConnection(_connectionString))
    {
        string sql = "SELECT * FROM Usuarios WHERE Email = @email AND Contraseña = @contrasena";
        Usuario usuario = db.QueryFirstOrDefault<Usuario>(sql, new { email, contrasena });
        return usuario;
    }
}

    public static int InsertarUsuario(Usuario usuario, string contrasena)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = @"INSERT INTO Usuarios
                                (Estado, FechaCreacionCuenta, Nombre, Apellido, Doc_nro, FechaNacimiento, Sexo, PesoEnKg, AlturaEnCm, Telefono, Email, Contraseña)
                                VALUES (@Estado, @FechaCreacionCuenta, @Nombre, @Apellido, @Doc_nro, @FechaNacimiento, @Sexo, @PesoEnKg, @AlturaEnCm, @Telefono, @Email, @Contrasena);
                                SELECT CAST(SCOPE_IDENTITY() AS int);";

            int nuevoId = db.ExecuteScalar<int>(sql, new
            {
                usuario.Estado,
                usuario.FechaCreacionCuenta,
                usuario.Nombre,
                usuario.Apellido,
                Doc_nro = usuario.Doc_nro,
                usuario.FechaNacimiento,
                usuario.Sexo,
                usuario.PesoEnKg,
                usuario.AlturaEnCm,
                usuario.Telefono,
                usuario.Email,
                Contrasena = contrasena
            });
            return nuevoId;
        }
    }


    public static Usuario ObtenerUsuarioPorId(int Id){
        using(SqlConnection db = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios
                            WHERE Id = @pId";
            Usuario usuario = db.QueryFirstOrDefault<Usuario>(query, new {pId = Id});
            return usuario;
        }
    }
    public static List<Diagnostico> ObtenerDiagnosticosPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT Id, IdUsuario, IdPatologia, Descripcion, FechaInicio, FechaFin, Estado, NombrePatologia
                           FROM Diagnosticos
                           WHERE IdUsuario = @idUsuario";
            List<Diagnostico> listaDiagnosticos = db.Query<Diagnostico>(sql, new { idUsuario }).ToList();
            return listaDiagnosticos;
        }
    }




    public static List<Alergia> ObtenerAlergiasPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Alergias WHERE IdUsuario = @idUsuario";
            List<Alergia> listaAlergias = db.Query<Alergia>(sql, new { idUsuario }).ToList();
            return listaAlergias;
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
            List<MedicacionesPaciente> listaMedicaciones = db.Query<MedicacionesPaciente>(sql, new { idUsuario }).ToList();
            return listaMedicaciones;
        }
    }




    public static List<dynamic> ObtenerVacunasPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT VXP.*, V.NombreVacuna, V.Dosis, V.Aplicacion
                           FROM VacunasXPaciente VXP
                           INNER JOIN Vacunas V ON VXP.IdVacuna = V.Id
                           WHERE VXP.IdUsuario = @idUsuario";
            List<dynamic> listaVacunas = db.Query<dynamic>(sql, new { idUsuario }).ToList();
            return listaVacunas;
        }
    }










    public static List<Organizacion> ObtenerOrganizaciones()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT O.*, D.Calle, D.Altura
                           FROM Organizaciones O
                           LEFT JOIN Direcciones D ON O.IdDireccion = D.Id";
            List<Organizacion> listaOrganizaciones = db.Query<Organizacion>(sql).ToList();
            return listaOrganizaciones;
        }
    }


    public static string ObtenerNombreOrganizacion(int idOrganizacion)
    {
        if (idOrganizacion <= 0)
            return null;

        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT Nombre FROM Organizaciones WHERE Id = @idOrganizacion";
            string nombre = db.QueryFirstOrDefault<string>(sql, new { idOrganizacion });
            return nombre;
        }
    }


    public static string ObtenerNombreVacuna(int idVacuna)
    {
        if (idVacuna <= 0)
            return null;

        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT NombreVacuna FROM Vacunas WHERE Id = @idVacuna";
            string nombre = db.QueryFirstOrDefault<string>(sql, new { idVacuna });
            return nombre;
        }
    }


    public static string ObtenerModalidadEstudio(int idModalidad)
    {
        if (idModalidad <= 0)
            return null;

        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            const string sql = "SELECT Tipo_ImagenEstudio FROM Modalidad WHERE Id = @idModalidad";
            string modalidad = db.QueryFirstOrDefault<string>(sql, new { idModalidad });
            return modalidad;
        }
    }
   
    public static List<Encuentro> ObtenerEncuentrosPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT E.*, O.Nombre AS NombreOrganizacion, TipoOrg.TipoOrganizacion AS Tipo
                           FROM Encuentros E
                           LEFT JOIN Organizaciones O ON E.IdOrganizacion = O.Id
                           LEFT JOIN Tipo_Organizacion TipoOrg ON O.Id_Tipo_Organizacion = TipoOrg.Id
                           WHERE E.IdUsuario = @idUsuario";
            List<Encuentro> listaEncuentros = db.Query<Encuentro>(sql, new { idUsuario }).ToList();
            return listaEncuentros;
        }
    }








    public static List<DocumentoClinico> ObtenerDocumentosPorEncuentro(int idEncuentro)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT DC.*, A.Capacidad, A.FechaCreacion
                           FROM Documentos_Clinicos DC
                           LEFT JOIN Archivos A ON DC.IdArchivo = A.Id
                           WHERE DC.IdEncuentro = @idEncuentro";
            List<DocumentoClinico> listaDocumentos = db.Query<DocumentoClinico>(sql, new { idEncuentro }).ToList();
            return listaDocumentos;
        }
    }




    public static List<Imagenes_Estudio> ObtenerImagenesPorEncuentro(int idEncuentro)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Imagenes_Estudios WHERE IdEncuentro = @idEncuentro";
            List<Imagenes_Estudio> listaImagenes = db.Query<Imagenes_Estudio>(sql, new { idEncuentro }).ToList();
            return listaImagenes;
        }
    }

    public static bool ActualizarCampoUsuario(int idUsuario, string campo, string valor)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            // Validar que el campo sea uno de los permitidos
            var camposPermitidos = new[] { "Nombre", "Apellido", "Email", "Doc_nro", "FechaNacimiento", 
                "Sexo", "PesoEnKg", "AlturaEnCm", "Telefono" };
            
            if (!camposPermitidos.Contains(campo))
            {
                return false;
            }

            // Construir la consulta SQL dinámicamente
            string sql = $"UPDATE Usuarios SET {campo} = @valor WHERE Id = @idUsuario";
            
            // Convertir el valor según el tipo de campo
            object valorConvertido = valor;
            
            if (campo == "Doc_nro" || campo == "Telefono")
            {
                if (int.TryParse(valor, out int valorInt))
                    valorConvertido = valorInt;
                else
                    return false;
            }
            else if (campo == "PesoEnKg" || campo == "AlturaEnCm")
            {
                if (double.TryParse(valor, out double valorDouble))
                    valorConvertido = valorDouble;
                else
                    return false;
            }
            else if (campo == "FechaNacimiento")
            {
                if (DateTime.TryParse(valor, out DateTime valorDateTime))
                    valorConvertido = valorDateTime;
                else
                    return false;
            }
            else if (campo == "Sexo")
            {
                if (valor.Length > 0)
                    valorConvertido = valor[0];
                else
                    return false;
            }

            int registrosAfectados = db.Execute(sql, new { valor = valorConvertido, idUsuario });
            return registrosAfectados > 0;
        }
    }
        public static bool ActualizarMedicamento(int id, int idUsuario, string nombreComercial, string dosis, string frecuencia, string horaProgramada, string indicacion)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"UPDATE MedicacionesPaciente 
                           SET Nombre_Comercial = @nombreComercial,
                               Dosis = @dosis,
                               Frecuencia = @frecuencia,
                               HoraProgramada = @horaProgramada,
                               Indicacion = @indicacion
                           WHERE Id = @id AND IdUsuario = @idUsuario";
            
            DateTime hora = DateTime.Today;
            if (TimeSpan.TryParse(horaProgramada, out TimeSpan time))
            {
                hora = DateTime.Today.Add(time);
            }
            
            int filasAfectadas = db.Execute(sql, new { id, idUsuario, nombreComercial, dosis, frecuencia, horaProgramada = hora, indicacion });
            return filasAfectadas > 0;
        }
    }

    public static bool EliminarMedicamento(int id, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"DELETE FROM MedicacionesPaciente WHERE Id = @id AND IdUsuario = @idUsuario";
            int filasAfectadas = db.Execute(sql, new { id, idUsuario });
            return filasAfectadas > 0;
        }
    }

}
