
using Microsoft.Data.SqlClient;
using Dapper;
using System.Collections.Generic;
using System.Linq;




public static class BD
{
    private static string _connectionString = @"Server=MSI\SQLEXPRESS03;Database=BDVitalink;Integrated Security=True;TrustServerCertificate=True;";


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


    public static Usuario ObtenerUsuarioPorId(int Id)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string query = @"SELECT * FROM Usuarios
                            WHERE Id = @pId";
            Usuario usuario = db.QueryFirstOrDefault<Usuario>(query, new { pId = Id });
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

    /// <summary>
    /// Obtiene la lista de todas las modalidades
    /// </summary>
    public static List<Modalidad> ObtenerModalidades()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Modalidad ORDER BY Tipo_ImagenEstudio";
            List<Modalidad> listaModalidades = db.Query<Modalidad>(sql).ToList();
            return listaModalidades;
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

    /// <summary>
    /// Elimina un encuentro de la base de datos
    /// </summary>
    public static bool EliminarEncuentro(int id, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"DELETE FROM Encuentros WHERE Id = @id AND IdUsuario = @idUsuario";
            int filasAfectadas = db.Execute(sql, new { id, idUsuario });
            return filasAfectadas > 0;
        }
    }

    public static bool RestarPastilla(int id, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"UPDATE MedicacionesPaciente 
                           SET Cantidad = CASE 
                               WHEN Cantidad > 0 THEN Cantidad - 1 
                               ELSE 0 
                           END
                           WHERE Id = @id AND IdUsuario = @idUsuario AND Cantidad > 0";
            int filasAfectadas = db.Execute(sql, new { id, idUsuario });
            return filasAfectadas > 0;
        }
    }

    public static int? ObtenerCantidadMedicamento(int id, int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT Cantidad FROM MedicacionesPaciente 
                           WHERE Id = @id AND IdUsuario = @idUsuario";
            int? cantidad = db.QueryFirstOrDefault<int?>(sql, new { id, idUsuario });
            return cantidad;
        }
    }

    public static int AgregarRecetaYDevolverId(string NombreMedico, string ApellidoMedico, DateTime FechaEmision, DateTime FechaCaducacion, string Observaciones)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            // Validar que las fechas estén en el rango válido de SQL Server
            DateTime fechaMinima = new DateTime(1753, 1, 1);
            DateTime fechaMaxima = new DateTime(9999, 12, 31);
            
            if (FechaEmision < fechaMinima || FechaEmision > fechaMaxima)
                FechaEmision = DateTime.Today; // Valor por defecto si está fuera de rango
            
            if (FechaCaducacion < fechaMinima || FechaCaducacion > fechaMaxima)
                FechaCaducacion = DateTime.Today.AddYears(1); // Valor por defecto
            
            string sql = @"INSERT INTO Recetas(
                            NombreMedico, ApellidoMedico, FechaEmision, FechaCaducacion, Observaciones
                            ) 
                            VALUES(
                                @pNombreMedico, @pApellidoMedico, @pFechaEmision, @pFechaCaducacion, @pObservaciones
                            );
                            SELECT CAST(SCOPE_IDENTITY() AS int);";
            
            int IdReceta = db.ExecuteScalar<int>(sql, new { 
                @pNombreMedico = NombreMedico ?? string.Empty, 
                @pApellidoMedico = ApellidoMedico ?? string.Empty, 
                @pFechaEmision = FechaEmision, 
                @pFechaCaducacion = FechaCaducacion, 
                @pObservaciones = Observaciones ?? string.Empty 
            });
            return IdReceta;
        }
    }

    public static int CrearRecetaPorDefecto()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO Recetas(
                            NombreMedico, ApellidoMedico, FechaEmision, FechaCaducacion, Observaciones
                            ) 
                            VALUES(
                                '', '', @pFechaEmision, @pFechaCaducacion, ''
                            );
                            SELECT CAST(SCOPE_IDENTITY() AS int);";
            
            DateTime fechaHoy = DateTime.Today;
            DateTime fechaFutura = DateTime.Today.AddYears(1);
            
            int IdReceta = db.ExecuteScalar<int>(sql, new { 
                @pFechaEmision = fechaHoy, 
                @pFechaCaducacion = fechaFutura
            });
            return IdReceta;
        }
    }

    public static void AgregarMedicacionPaciente(MedicacionesPaciente medicacion)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO MedicacionesPaciente (
                                IdReceta, IdUsuario, Nombre_Comercial, Dosis, Via, Frecuencia,
                                Indicacion, HoraProgramada, FechaFabricacion, FechaVencimiento, Estado, Cantidad
                            )
                            VALUES (
                                @IdReceta, @IdUsuario, @Nombre_Comercial, @Dosis, @Via, @Frecuencia,
                                @Indicacion, @HoraProgramada, @FechaFabricacion, @FechaVencimiento, @Estado, @Cantidad
                            )";
            db.Execute(sql, medicacion);
        }
    }

    public static List<dynamic> ObtenerEncuentrosConDireccionPorUsuario(int idUsuario)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"SELECT E.*, O.Nombre AS NombreOrganizacion, TipoOrg.TipoOrganizacion AS Tipo,
                           CONCAT(D.Calle, ' ', D.Altura) AS Direccion
                           FROM Encuentros E
                           LEFT JOIN Organizaciones O ON E.IdOrganizacion = O.Id
                           LEFT JOIN Tipo_Organizacion TipoOrg ON O.Id_Tipo_Organizacion = TipoOrg.Id
                           LEFT JOIN Direcciones D ON O.IdDireccion = D.Id
                           WHERE E.IdUsuario = @idUsuario";
            List<dynamic> listaEncuentros = db.Query<dynamic>(sql, new { idUsuario }).ToList();
            return listaEncuentros;
        }
    }

    // ==================== FUNCIONES PARA AGREGAR MANUALMENTE ====================

    /// <summary>
    /// Inserta una vacunación manual usando el SP sp_InsertVacunacionManual
    /// </summary>
    public static int InsertarVacunacionManual(int idUsuario, string dosis, string nombreVacuna,
        string datosInteres, DateTime fechaAplicacion, string nombreOrganizacion, int idTipoOrganizacion)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@IdUsuario", idUsuario);
            parameters.Add("@Dosis", dosis);
            parameters.Add("@NombreVacuna", nombreVacuna);
            parameters.Add("@DatosInteres", datosInteres);
            parameters.Add("@FechaAplicacion", fechaAplicacion);
            parameters.Add("@NombreOrganizacion", nombreOrganizacion);
            parameters.Add("@IdTipoOrganizacion", idTipoOrganizacion);

            // El SP devuelve el ID de la vacunación en un SELECT
            int idVacunaPaciente = db.QuerySingle<int>(
                "sp_InsertVacunacionManual", 
                parameters, 
                commandType: System.Data.CommandType.StoredProcedure
            );

            return idVacunaPaciente;
        }
    }

    /// <summary>
    /// Inserta un estudio manual usando el SP sp_InsertEstudioManual
    /// </summary>
    public static int InsertarEstudioManual(int idUsuario, string nombreEstudio, int idModalidad,
        string observacion, DateTime fecha, string capacidad = null, DateTime? fechaCreacionArchivo = null,
        string nombreArchivo = null, string tipoArchivo = null)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@IdUsuario", idUsuario);
            parameters.Add("@NombreEstudio", nombreEstudio);
            parameters.Add("@IdModalidad", idModalidad);
            parameters.Add("@Observacion", observacion);
            parameters.Add("@Fecha", fecha);
            parameters.Add("@Capacidad", capacidad);
            parameters.Add("@FechaCreacionArchivo", fechaCreacionArchivo);
            parameters.Add("@NombreArchivo", nombreArchivo);
            parameters.Add("@TipoArchivo", tipoArchivo);

            // El SP devuelve el ID del estudio en un SELECT
            int idEstudio = db.QuerySingle<int>(
                "sp_InsertEstudioManual", 
                parameters, 
                commandType: System.Data.CommandType.StoredProcedure
            );

            return idEstudio;
        }
    }

    /// <summary>
    /// Inserta una enfermedad manual usando el SP sp_InsertEnfermedadManual
    /// </summary>
    public static int InsertarEnfermedadManual(int idUsuario, string nombreEnfermedad,
        string descripcion, DateTime fecha)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            var parameters = new DynamicParameters();
            parameters.Add("@IdUsuario", idUsuario);
            parameters.Add("@NombreEnfermedad", nombreEnfermedad);
            parameters.Add("@Descripcion", descripcion);
            parameters.Add("@Fecha", fecha);

            // El SP devuelve el ID del diagnóstico en un SELECT
            int idDiagnostico = db.QuerySingle<int>(
                "sp_InsertEnfermedadManual", 
                parameters, 
                commandType: System.Data.CommandType.StoredProcedure
            );

            return idDiagnostico;
        }
    }

    // ==================== FUNCIONES PARA ENCUENTROS ====================

    /// <summary>
    /// Obtiene la lista de tipos de organización
    /// </summary>
    public static List<Tipo_Organizacion> ObtenerTiposOrganizacion()
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT * FROM Tipo_Organizacion ORDER BY TipoOrganizacion";
            List<Tipo_Organizacion> listaTipos = db.Query<Tipo_Organizacion>(sql).ToList();
            return listaTipos;
        }
    }

    /// <summary>
    /// Busca una organización por nombre y retorna su Id, o null si no existe
    /// </summary>
    public static int? ObtenerIdOrganizacionPorNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return null;

        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = "SELECT Id FROM Organizaciones WHERE Nombre = @nombre";
            int? id = db.QueryFirstOrDefault<int?>(sql, new { nombre = nombre.Trim() });
            return id;
        }
    }

    /// <summary>
    /// Inserta una nueva dirección y retorna el Id generado
    /// </summary>
    public static int InsertarDireccion(string calle, string altura)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO Direcciones (Calle, Altura)
                           VALUES (@calle, @altura);
                           SELECT CAST(SCOPE_IDENTITY() AS int);";
            
            int idDireccion = db.ExecuteScalar<int>(sql, new 
            { 
                calle = calle ?? string.Empty, 
                altura = altura ?? string.Empty 
            });
            return idDireccion;
        }
    }

    /// <summary>
    /// Inserta una nueva organización y retorna el Id generado
    /// </summary>
    public static int InsertarOrganizacion(string nombre, int idTipoOrganizacion, int idDireccion)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO Organizaciones (Nombre, Id_Tipo_Organizacion, IdDireccion)
                           VALUES (@nombre, @idTipoOrganizacion, @idDireccion);
                           SELECT CAST(SCOPE_IDENTITY() AS int);";
            
            int idOrganizacion = db.ExecuteScalar<int>(sql, new 
            { 
                nombre = nombre ?? string.Empty, 
                idTipoOrganizacion, 
                idDireccion 
            });
            return idOrganizacion;
        }
    }

    /// <summary>
    /// Busca una organización por nombre; si existe retorna su Id, si no existe crea la dirección (si se proporciona) y la organización, luego retorna el Id
    /// </summary>
    public static int ObtenerOCrearOrganizacion(string nombre, int idTipoOrganizacion, string calle = null, string altura = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la organización es requerido");

        // Primero buscar si existe
        int? idExistente = ObtenerIdOrganizacionPorNombre(nombre);
        if (idExistente.HasValue)
            return idExistente.Value;

        // Si no existe, crear la organización
        int idDireccion = 0;
        
        // Si se proporciona calle o altura, crear la dirección
        if (!string.IsNullOrWhiteSpace(calle) || !string.IsNullOrWhiteSpace(altura))
        {
            idDireccion = InsertarDireccion(calle ?? string.Empty, altura ?? string.Empty);
        }

        // Crear la organización
        int idOrganizacion = InsertarOrganizacion(nombre, idTipoOrganizacion, idDireccion);
        return idOrganizacion;
    }

    /// <summary>
    /// Inserta un nuevo encuentro en la BD y retorna el Id generado
    /// </summary>
    public static int InsertarEncuentro(Encuentro encuentro)
    {
        using (SqlConnection db = new SqlConnection(_connectionString))
        {
            string sql = @"INSERT INTO Encuentros (IdUsuario, IdOrganizacion, NombreMedico, ApellidoMedico, FechaInicio, FechaFin, EstadoMotivo)
                           VALUES (@IdUsuario, @IdOrganizacion, @NombreMedico, @ApellidoMedico, @FechaInicio, @FechaFin, @EstadoMotivo);
                           SELECT CAST(SCOPE_IDENTITY() AS int);";
            
            int idEncuentro = db.ExecuteScalar<int>(sql, new 
            { 
                encuentro.IdUsuario,
                encuentro.IdOrganizacion,
                NombreMedico = encuentro.NombreMedico ?? string.Empty,
                ApellidoMedico = encuentro.ApellidoMedico ?? string.Empty,
                encuentro.FechaInicio,
                FechaFin = encuentro.FechaFin,
                EstadoMotivo = encuentro.EstadoMotivo ?? string.Empty
            });
            return idEncuentro;
        }
    }
}
