using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VitaLink.Models;


namespace VitaLink.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;


    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        ViewBag.FormData = new Dictionary<string, string>();
        return View("SignUp");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SignUp(
        string nombre,
        string apellido,
        string email,
        string contrasena,
        string peso,
        string altura,
        string sexo,
        string telefono,
        string fechaNacimiento,
        string numeroDocumento)
    {
        var errores = new List<string>();
        var formData = new Dictionary<string, string>
        {
            ["Nombre"] = nombre,
            ["Apellido"] = apellido,
            ["Email"] = email,
            ["Contrasena"] = contrasena,
            ["Peso"] = peso,
            ["Altura"] = altura,
            ["Sexo"] = sexo,
            ["Telefono"] = telefono,
            ["FechaNacimiento"] = fechaNacimiento,
            ["NumeroDocumento"] = numeroDocumento
        };

        if (string.IsNullOrWhiteSpace(nombre)) errores.Add("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(apellido)) errores.Add("El apellido es obligatorio.");
        if (string.IsNullOrWhiteSpace(email)) errores.Add("El email es obligatorio.");
        if (string.IsNullOrWhiteSpace(contrasena)) errores.Add("La contraseña es obligatoria.");
        if (string.IsNullOrWhiteSpace(sexo)) errores.Add("El sexo es obligatorio.");

        if (!double.TryParse(peso, out double pesoEnKg) || pesoEnKg <= 0)
            errores.Add("Ingrese un peso válido.");

        if (!double.TryParse(altura, out double alturaEnCm) || alturaEnCm <= 0)
            errores.Add("Ingrese una altura válida.");

        if (!DateTime.TryParse(fechaNacimiento, out DateTime fechaNacValida))
            errores.Add("Ingrese una fecha de nacimiento válida.");

        if (!int.TryParse(numeroDocumento, out int docNro) || docNro <= 0)
            errores.Add("Ingrese un número de documento válido.");

        if (!long.TryParse(telefono, out long telefonoValido) || telefonoValido <= 0 || telefonoValido > int.MaxValue)
            errores.Add("Ingrese un teléfono válido.");

        char sexoFormateado = 'N';
        if (!string.IsNullOrWhiteSpace(sexo))
        {
            sexoFormateado = char.ToUpperInvariant(sexo[0]);
            if (sexoFormateado != 'M' && sexoFormateado != 'F' && sexoFormateado != 'O' && sexoFormateado != 'N')
            {
                errores.Add("Seleccione un sexo válido (M, F, O, N).");
            }
        }

        if (errores.Count > 0)
        {
            ViewBag.Errors = errores;
            ViewBag.FormData = formData;
            return View("SignUp");
        }

        var nuevoUsuario = new Usuario
        {
            Estado = false,
            FechaCreacionCuenta = DateTime.Now,
            Nombre = nombre,
            Apellido = apellido,
            Doc_nro = docNro,
            FechaNacimiento = fechaNacValida,
            Sexo = sexoFormateado,
            PesoEnKg = pesoEnKg,
            AlturaEnCm = alturaEnCm,
            Telefono = (int)telefonoValido,
            Email = email
        };

        try
        {
            int nuevoId = BD.InsertarUsuario(nuevoUsuario, contrasena);
            if (nuevoId > 0)
            {
                TempData["SignUpSuccess"] = "Cuenta creada con éxito. Ahora puedes iniciar sesión.";
                return RedirectToAction("LogIn");
            }

            ViewBag.Errors = new List<string> { "No se pudo crear la cuenta. Inténtalo nuevamente." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar un nuevo usuario");
            ViewBag.Errors = new List<string> { "Ocurrió un error al crear la cuenta. Por favor, intenta más tarde." };
        }

        ViewBag.FormData = formData;
        return View("SignUp");
    }


    public IActionResult Index()
    {
        return View("Index");
    }

    [HttpGet]
    public IActionResult LogIn()
    {
        ViewBag.SignUpSuccess = TempData["SignUpSuccess"];
        return View("LogIn");
    }
    private void ActualizarSession(Usuario usuario){
        HttpContext.Session.SetString("User", Objeto.ObjectToString(usuario));
        HttpContext.Session.SetInt32("IdUsuario", usuario.Id);
    }
    
    private Usuario ObtenerUsuario(){
        string usuarioJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(usuarioJson))
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario > 0)
            {
                Usuario usuario = BD.ObtenerUsuarioPorId(idUsuario);
                if (usuario != null)
                {
                    ActualizarSession(usuario);
                    return usuario;
                }
            }
            return null;
        }
        return Objeto.StringToObject<Usuario>(usuarioJson);
    }
    public IActionResult Calendario()
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        ViewBag.Encuentros = BD.ObtenerEncuentrosConDireccionPorUsuario(usuario.Id);
        ViewBag.TiposOrganizacion = BD.ObtenerTiposOrganizacion();
        return View("Calendario");
    }
    public IActionResult Familia()
    {
        return View("Familia");
    }


    public IActionResult Medicamentos()
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        ViewBag.Medicaciones = BD.ObtenerMedicacionesPorUsuario(usuario.Id);
        return View("Medicamentos");
    }

    [HttpGet]
    public IActionResult Agregar()
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        // Inicializar un objeto MedicacionesPaciente con valores por defecto
        // HoraProgramada solo con horas y minutos (sin segundos ni milisegundos)
        DateTime horaActual = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 
                                            DateTime.Now.Hour, DateTime.Now.Minute, 0);
        
        var model = new MedicacionesPaciente
        {
            IdReceta = null, // Se asignará cuando se cree la receta, o null si no hay receta
            IdUsuario = usuario.Id, // Se obtiene de sesión
            Nombre_Comercial = string.Empty,
            Dosis = string.Empty,
            Via = string.Empty,
            Frecuencia = string.Empty,
            Indicacion = string.Empty,
            HoraProgramada = horaActual,
            FechaFabricacion = DateTime.Today,
            FechaVencimiento = DateTime.Today,
            Estado = true, // Siempre activo por defecto
            Cantidad = 0
        };
        return View("AgregarMedicamento", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Agregar(
        MedicacionesPaciente item,
        string NombreMedico,
        string ApellidoMedico,
        DateTime? FechaEmision,
        DateTime? FechaCaducacion,
        string Observaciones,
        bool AgregarReceta = false)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        item.IdUsuario = usuario.Id; // Siempre de sesión

        int? idReceta;
        
        // Si el usuario quiere agregar receta completa Y proporcionó datos válidos
        if (AgregarReceta && 
            !string.IsNullOrWhiteSpace(NombreMedico) &&
            FechaEmision.HasValue && 
            FechaCaducacion.HasValue)
        {
            // Validar que las fechas estén en rango válido
            DateTime fechaMinima = new DateTime(1753, 1, 1);
            DateTime fechaMaxima = new DateTime(9999, 12, 31);
            
            DateTime fechaEmisionValida = FechaEmision.Value;
            DateTime fechaCaducacionValida = FechaCaducacion.Value;
            
            if (fechaEmisionValida < fechaMinima || fechaEmisionValida > fechaMaxima)
                fechaEmisionValida = DateTime.Today;
            
            if (fechaCaducacionValida < fechaMinima || fechaCaducacionValida > fechaMaxima)
                fechaCaducacionValida = DateTime.Today.AddYears(1);
            
            // Crear receta con datos proporcionados
            idReceta = BD.AgregarRecetaYDevolverId(
                NombreMedico, 
                ApellidoMedico ?? string.Empty, 
                fechaEmisionValida, 
                fechaCaducacionValida, 
                Observaciones ?? string.Empty
            );
        }
        else
        {
            // Si no se agrega receta, IdReceta = null
            idReceta = null;
        }
        
        item.IdReceta = idReceta; // null si no hay receta, o el ID de la receta creada
        item.Estado = true; // Siempre activo cuando se agrega un medicamento nuevo

        if (ModelState.IsValid)
        {
            BD.AgregarMedicacionPaciente(item);
            return RedirectToAction("Medicamentos");
        }
        return View("AgregarMedicamento", item);
    }


 [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogIn(string email, string contrasena)
        {
            Usuario usuario = BD.LoginUsuario(email, contrasena);


            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos";
                return View("LogIn");
            }

            ActualizarSession(usuario);

            return RedirectToAction("Home");
        }




        public IActionResult Home()
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("LogIn");

            ViewBag.Usuario = usuario;
            var medicaciones = BD.ObtenerMedicacionesPorUsuario(usuario.Id);
            ViewBag.Medicaciones = medicaciones;
            ViewBag.Encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);

            // Generar notificaciones para medicamentos sin pastillas
            var notificaciones = new List<string>();
            if (medicaciones != null)
            {
                foreach (var med in medicaciones)
                {
                    if (med.Cantidad <= 0)
                    {
                        notificaciones.Add($"Te quedaste sin pastillas de: {med.Nombre_Comercial}");
                    }
                }
            }

            // Generar notificaciones para eventos próximos
            var encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);
            var hoy = DateTime.Today;
            
            if (encuentros != null)
            {
                foreach (var encuentro in encuentros)
                {
                    // Obtener solo la fecha (sin hora) del evento
                    var fechaEvento = encuentro.FechaInicio.Date;
                    
                    // Solo considerar eventos futuros o del día de hoy
                    if (fechaEvento >= hoy)
                    {
                        // Calcular días hasta el evento
                        int diasHastaEvento = (fechaEvento - hoy).Days;
                        
                        // Agregar notificación si es hoy (0 días), mañana (1 día) o en 5 días
                        if (diasHastaEvento == 0)
                        {
                            notificaciones.Add("Hoy tenes un evento");
                        }
                        else if (diasHastaEvento == 1)
                        {
                            notificaciones.Add("En 1 día tenes un evento");
                        }
                        else if (diasHastaEvento == 5)
                        {
                            notificaciones.Add("En 5 días tenes un evento");
                        }
                    }
                    // Si el evento ya pasó (fechaEvento < hoy), no se agrega notificación
                }
            }

            ViewBag.Notificaciones = notificaciones;

            return View("Home");
        }




        public IActionResult Perfil()
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("LogIn");

            ViewBag.Usuario = usuario;
            ViewBag.Alergias = BD.ObtenerAlergiasPorUsuario(usuario.Id);
            ViewBag.Diagnosticos = BD.ObtenerDiagnosticosPorUsuario(usuario.Id);

            return View("Perfil");
        }

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LogIn");
        }


   
        public IActionResult HistorialMedico()
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("LogIn");

            ViewBag.Usuario = usuario;
            ViewBag.Diagnosticos = BD.ObtenerDiagnosticosPorUsuario(usuario.Id);
            ViewBag.Vacunas = BD.ObtenerVacunasPorUsuario(usuario.Id);

            var encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);
            ViewBag.Documentos = encuentros
                .SelectMany(e => BD.ObtenerDocumentosPorEncuentro(e.Id))
                .ToList();

            ViewBag.AntecedentesFamiliares = new List<object>(); // placeholder
            return View("HistorialMedico");
        }




        [HttpPost]
        public IActionResult CambiarContrasena([FromBody] CambioContrasenaRequest request)
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) 
                return Json(new { success = false, message = "No hay sesión activa" });

            bool resultado = usuario.CambiarContraseña(request.ContrasenaActual, request.ContrasenaNueva);

            if (resultado)
            {
                ActualizarSession(usuario);
            }

            if (!resultado && usuario.IntentosFallidos >= 5)
            {
                return Json(new { success = false, bloqueado = true, message = "Se acabaron los intentos. Inténtelo más tarde." });
            }

            return Json(new { success = resultado, bloqueado = false });
        }


        public class CambioContrasenaRequest
        {
            public string ContrasenaActual { get; set; }
            public string ContrasenaNueva { get; set; }
        }

        public IActionResult EditarPerfil()
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) return RedirectToAction("LogIn");

            ViewBag.Usuario = usuario;
            return View("EditarPerfil");
        }

        [HttpPost]
        public IActionResult ActualizarCampoUsuario([FromBody] ActualizarCampoRequest request)
        {
            Usuario usuario = ObtenerUsuario();
            if (usuario == null) 
                return Json(new { success = false, message = "No hay sesión activa" });

            try
            {
                bool resultado = BD.ActualizarCampoUsuario(usuario.Id, request.Campo, request.Valor);
                if (resultado)
                {
                    // Actualizar el usuario en la sesión después de modificar
                    Usuario usuarioActualizado = BD.ObtenerUsuarioPorId(usuario.Id);
                    if (usuarioActualizado != null)
                    {
                        ActualizarSession(usuarioActualizado);
                    }
                    return Json(new { success = true, message = "Campo actualizado correctamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al actualizar el campo" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar campo del usuario");
                return Json(new { success = false, message = "Ocurrió un error al actualizar el campo" });
            }
        }

        public class ActualizarCampoRequest
        {
            public string Campo { get; set; }
            public string Valor { get; set; }
        }

        public IActionResult AgregarManualmente()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return RedirectToAction("LogIn");

            ViewBag.TiposOrganizacion = BD.ObtenerTiposOrganizacion();
            return View("AgregarManualmente");
        }

        public IActionResult AgregarAutomaticamente()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return RedirectToAction("LogIn");

            return View("AgregarAutomaticamente");
        }
        [HttpPost]
    public IActionResult ActualizarMedicamento([FromBody] ActualizarMedicamentoRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            bool resultado = BD.ActualizarMedicamento(request.Id, usuario.Id, request.Nombre_Comercial, 
                request.Dosis, request.Frecuencia, request.HoraProgramada, request.Indicacion);
            
            if (resultado)
                return Json(new { success = true, message = "Medicamento actualizado correctamente" });
            else
                return Json(new { success = false, message = "No se pudo actualizar el medicamento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar medicamento");
            return Json(new { success = false, message = "Error al actualizar el medicamento" });
        }
    }

    [HttpPost]
    public IActionResult EliminarMedicamento([FromBody] EliminarMedicamentoRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            bool resultado = BD.EliminarMedicamento(request.Id, usuario.Id);
            
            if (resultado)
                return Json(new { success = true, message = "Medicamento eliminado correctamente" });
            else
                return Json(new { success = false, message = "No se pudo eliminar el medicamento" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar medicamento");
            return Json(new { success = false, message = "Error al eliminar el medicamento" });
        }
    }

    [HttpPost]
    public IActionResult EliminarEncuentro([FromBody] EliminarEncuentroRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            bool resultado = BD.EliminarEncuentro(request.Id, usuario.Id);
            if (resultado)
            {
                return Json(new { success = true, message = "Evento eliminado correctamente" });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo eliminar el evento" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar encuentro");
            return Json(new { success = false, message = "Error al eliminar el evento" });
        }
    }

    [HttpPost]
    public IActionResult TomarMedicamento([FromBody] EliminarMedicamentoRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            bool resultado = BD.RestarPastilla(request.Id, usuario.Id);
            
            if (resultado)
            {
                int? cantidadRestante = BD.ObtenerCantidadMedicamento(request.Id, usuario.Id);
                return Json(new { success = true, message = "Medicamento registrado", cantidad = cantidadRestante ?? 0 });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo registrar la toma o no hay pastillas disponibles" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al tomar medicamento");
            return Json(new { success = false, message = "Error al registrar la toma del medicamento" });
        }
    }

    public class ActualizarMedicamentoRequest
    {
        public int Id { get; set; }
        public string Nombre_Comercial { get; set; }
        public string Dosis { get; set; }
        public string Frecuencia { get; set; }
        public string HoraProgramada { get; set; }
        public string Indicacion { get; set; }
    }

    public class EliminarMedicamentoRequest
    {
        public int Id { get; set; }
    }

    public class EliminarEncuentroRequest
    {
        public int Id { get; set; }
    }

    // ==================== ENDPOINTS PARA AGREGAR MANUALMENTE ====================

    [HttpPost]
    public IActionResult InsertarVacunacion([FromBody] VacunacionRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            if (string.IsNullOrWhiteSpace(request.NombreVacuna))
                return Json(new { success = false, message = "El nombre de la vacuna es obligatorio" });

            if (string.IsNullOrWhiteSpace(request.NombreOrganizacion))
                return Json(new { success = false, message = "El nombre de la organización es obligatorio" });

            if (request.IdTipoOrganizacion <= 0)
                return Json(new { success = false, message = "Debe seleccionar un tipo de organización" });

            int idVacunaPaciente = BD.InsertarVacunacionManual(
                usuario.Id,
                request.Dosis,
                request.NombreVacuna,
                request.DatosInteres,
                request.FechaAplicacion,
                request.NombreOrganizacion,
                request.IdTipoOrganizacion
            );

            return Json(new { success = true, message = "Vacunación agregada correctamente", id = idVacunaPaciente });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar vacunación");
            return Json(new { success = false, message = "Error al agregar la vacunación" });
        }
    }

    [HttpPost]
    public IActionResult InsertarEstudio([FromBody] EstudioRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            if (string.IsNullOrWhiteSpace(request.NombreEstudio))
                return Json(new { success = false, message = "El nombre del estudio es obligatorio" });

            int idEstudio = BD.InsertarEstudioManual(
                usuario.Id,
                request.NombreEstudio,
                request.Observacion,
                request.Fecha,
                request.Capacidad,
                request.FechaCreacionArchivo,
                request.NombreArchivo,
                request.TipoArchivo
            );

            return Json(new { success = true, message = "Estudio agregado correctamente", id = idEstudio });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar estudio: {Message}", ex.Message);
            return Json(new { success = false, message = $"Error al agregar el estudio: {ex.Message}" });
        }
    }

    [HttpPost]
    public IActionResult InsertarEnfermedad([FromBody] EnfermedadRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            if (string.IsNullOrWhiteSpace(request.NombreEnfermedad))
                return Json(new { success = false, message = "El nombre de la enfermedad es obligatorio" });

            int idDiagnostico = BD.InsertarEnfermedadManual(
                usuario.Id,
                request.NombreEnfermedad,
                request.Descripcion,
                request.Fecha
            );

            return Json(new { success = true, message = "Enfermedad agregada correctamente", id = idDiagnostico });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar enfermedad");
            return Json(new { success = false, message = "Error al agregar la enfermedad" });
        }
    }

    // ==================== ENDPOINTS PARA ENCUENTROS ====================

    [HttpGet]
    public IActionResult ObtenerTiposOrganizacion()
    {
        try
        {
            var tipos = BD.ObtenerTiposOrganizacion();
            return Json(new { success = true, tipos = tipos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de organización");
            return Json(new { success = false, message = "Error al obtener tipos de organización" });
        }
    }

    [HttpPost]
    public IActionResult AgregarEncuentro([FromBody] EncuentroRequest request)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null)
            return Json(new { success = false, message = "No hay sesión activa" });

        try
        {
            // Validar que el request no sea null
            if (request == null)
                return Json(new { success = false, message = "Los datos del formulario son inválidos" });
            // Validaciones
            if (string.IsNullOrWhiteSpace(request.NombreMedico))
                return Json(new { success = false, message = "El nombre del médico es obligatorio" });

            if (string.IsNullOrWhiteSpace(request.ApellidoMedico))
                return Json(new { success = false, message = "El apellido del médico es obligatorio" });

            if (string.IsNullOrWhiteSpace(request.EstadoMotivo))
                return Json(new { success = false, message = "El estado/motivo es obligatorio" });

            if (string.IsNullOrWhiteSpace(request.NombreOrganizacion))
                return Json(new { success = false, message = "El nombre de la organización es obligatorio" });

            if (request.IdTipoOrganizacion <= 0)
                return Json(new { success = false, message = "Debe seleccionar un tipo de organización" });

            // Obtener o crear la organización
            int idOrganizacion = BD.ObtenerOCrearOrganizacion(
                request.NombreOrganizacion,
                request.IdTipoOrganizacion,
                request.Calle,
                request.Altura
            );

            // Crear el encuentro
            var encuentro = new Encuentro
            {
                IdUsuario = usuario.Id,
                IdOrganizacion = idOrganizacion,
                NombreMedico = request.NombreMedico,
                ApellidoMedico = request.ApellidoMedico,
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin,
                EstadoMotivo = request.EstadoMotivo
            };

            int idEncuentro = BD.InsertarEncuentro(encuentro);

            // Construir la dirección si se proporcionó calle o altura
            string direccion = "";
            if (!string.IsNullOrWhiteSpace(request.Calle) || !string.IsNullOrWhiteSpace(request.Altura))
            {
                direccion = $"{request.Calle ?? ""} {request.Altura ?? ""}".Trim();
            }

            // Retornar los datos del encuentro agregado para que aparezca en el calendario
            return Json(new 
            { 
                success = true, 
                message = "Encuentro agregado correctamente",
                encuentro = new
                {
                    id = idEncuentro,
                    fecha = request.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    nombreMedico = request.NombreMedico ?? "",
                    apellidoMedico = request.ApellidoMedico ?? "",
                    nombreOrganizacion = request.NombreOrganizacion ?? "",
                    direccion = direccion,
                    descripcion = request.EstadoMotivo ?? ""
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar encuentro");
            return Json(new { success = false, message = "Error al agregar el encuentro: " + ex.Message });
        }
    }

    // ==================== REQUEST CLASSES ====================

    public class VacunacionRequest
    {
        public string Dosis { get; set; }
        public string NombreVacuna { get; set; }
        public string DatosInteres { get; set; }
        public DateTime FechaAplicacion { get; set; }
        public string NombreOrganizacion { get; set; }
        public int IdTipoOrganizacion { get; set; }
    }

    public class EstudioRequest
    {
        public string NombreEstudio { get; set; }
        public string Observacion { get; set; }
        public DateTime Fecha { get; set; }
        public string Capacidad { get; set; }
        public DateTime? FechaCreacionArchivo { get; set; }
        public string NombreArchivo { get; set; }
        public string TipoArchivo { get; set; }
    }

    public class EnfermedadRequest
    {
        public string NombreEnfermedad { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
    }

    public class EncuentroRequest
    {
        public string NombreMedico { get; set; }
        public string ApellidoMedico { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string EstadoMotivo { get; set; }
        public string NombreOrganizacion { get; set; }
        public int IdTipoOrganizacion { get; set; }
        public string Calle { get; set; }
        public string Altura { get; set; }
    }

    // ==================== ENDPOINTS PARA DOCUMENTOS PDF ====================

    [HttpGet]
    public IActionResult DescargarDocumento(int id, int? idArchivo)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        // Verificar que el documento pertenece al usuario
        var encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);
        var documentos = encuentros.SelectMany(e => BD.ObtenerDocumentosPorEncuentro(e.Id)).ToList();
        var documento = documentos.FirstOrDefault(d => d.Id == id);

        if (documento == null)
        {
            return NotFound("Documento no encontrado");
        }

        if (!idArchivo.HasValue || idArchivo.Value <= 0)
        {
            return BadRequest("No hay archivo asociado a este documento");
        }

        // Por ahora, retornamos un mensaje ya que no tenemos el sistema de archivos implementado
        return Json(new { message = "Funcionalidad de descarga en desarrollo. El archivo se descargará cuando se implemente el almacenamiento de archivos." });
    }

    [HttpGet]
    public IActionResult VerDocumento(int id, int? idArchivo)
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        // Verificar que el documento pertenece al usuario
        var encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);
        var documentos = encuentros.SelectMany(e => BD.ObtenerDocumentosPorEncuentro(e.Id)).ToList();
        var documento = documentos.FirstOrDefault(d => d.Id == id);

        if (documento == null)
        {
            return NotFound("Documento no encontrado");
        }

        if (!idArchivo.HasValue || idArchivo.Value <= 0)
        {
            return BadRequest("No hay archivo asociado a este documento");
        }

        // Por ahora, retornamos una vista con información del documento
        // En producción, aquí se leería el archivo PDF y se mostraría en un iframe o viewer
        ViewBag.Documento = documento;
        return View("VerDocumento");
    }

}
     

