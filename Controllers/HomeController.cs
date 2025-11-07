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

    public IActionResult Index()
    {
        return View();
    }
    /*
    private void ActualizarSession(){
        HttpContext.Session.SetString("User", Objeto.ObjectToString());
    }
    private Usuario ObtenerUsuario(){
        return Objeto.StringToObject(Http.Context.Session.GetString("User"));
    }

*/
    public IActionResult Calendario()
    {
        return View("Calendario");
    }
    public IActionResult Crianzas()
    {
        return View("Crianzas");
    }

    public IActionResult Medicamentos()
    {
        return View("Medicamentos");
    }

 [HttpPost]
        public IActionResult LogIn(string email, string contrasena)
        {
            Usuario usuario = BD.LoginUsuario(email, contrasena);

            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos";
                return View("LogIn");
            }

            HttpContext.Session.SetInt32("IdUsuario", usuario.Id);

            return RedirectToAction("Home");
        }


        public IActionResult Home()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return RedirectToAction("LogIn");

            Usuario usuario = BD.ObtenerUsuarioPorId(idUsuario);

            ViewBag.Usuario = usuario;
            ViewBag.Medicaciones = BD.ObtenerMedicacionesPorUsuario(idUsuario);
            ViewBag.Encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
            ViewBag.Notificaciones = GenerarNotificaciones(idUsuario);

            return View("Home");
        }


        public IActionResult Perfil()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return RedirectToAction("LogIn");

            ViewBag.Usuario = BD.ObtenerUsuarioPorId(idUsuario);
            ViewBag.Alergias = BD.ObtenerAlergiasPorUsuario(idUsuario);
            ViewBag.Diagnosticos = BD.ObtenerDiagnosticosPorUsuario(idUsuario);

            return View("Perfil");
        }

   
        public IActionResult HistorialMedico()
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return RedirectToAction("LogIn");

            ViewBag.Usuario = BD.ObtenerUsuarioPorId(idUsuario);
            ViewBag.Diagnosticos = BD.ObtenerDiagnosticosPorUsuario(idUsuario);
            ViewBag.Vacunas = BD.ObtenerVacunasPorUsuario(idUsuario);

            var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
            ViewBag.Documentos = encuentros
                .SelectMany(e => BD.ObtenerDocumentosPorEncuentro(e.Id))
                .ToList();

            ViewBag.AntecedentesFamiliares = new List<object>(); // placeholder
            return View("HistorialMedico");
        }


        [HttpPost]
        public IActionResult CambiarContrasena([FromBody] CambioContrasenaRequest request)
        {
            int idUsuario = HttpContext.Session.GetInt32("IdUsuario") ?? 0;
            if (idUsuario == 0) return Json(new { success = false, message = "No hay sesión activa" });

            Usuario usuario = BD.ObtenerUsuarioPorId(idUsuario);
            if (usuario == null)
                return Json(new { success = false, message = "Usuario no encontrado" });

            bool resultado = usuario.CambiarContraseña(request.ContrasenaActual, request.ContrasenaNueva);

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


        private List<string> GenerarNotificaciones(int idUsuario)
        {
            var notificaciones = new List<string>();
            var medicaciones = BD.ObtenerMedicacionesPorUsuario(idUsuario);
            var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
            var vacunas = BD.ObtenerVacunasPorUsuario(idUsuario);
            var encuentrosConEstudios = BD.ObtenerEncuentrosPorUsuario(idUsuario);
            DateTime ahora = DateTime.Now;

            foreach (var med in medicaciones)
            {
                if (DateTime.TryParse(med.HoraProgramada, out DateTime horaMedicacion))
                {
                    TimeSpan diff = horaMedicacion - ahora;
                    if (diff.TotalMinutes <= 10 && diff.TotalMinutes > 5)
                        notificaciones.Add($"En 10 minutos debes tomar {med.Nombre_Comercial} ({med.Dosis}).");
                    else if (diff.TotalMinutes <= 5 && diff.TotalMinutes > 0)
                        notificaciones.Add($"En 5 minutos debes tomar {med.Nombre_Comercial} ({med.Dosis}).");
                    else if (Math.Abs(diff.TotalMinutes) < 1)
                        notificaciones.Add($"Es hora de tomar {med.Nombre_Comercial} ({med.Dosis}).");
                }
            }

            foreach (var enc in encuentros)
            {
                if (enc.FechaInicio.HasValue)
                {
                    double dias = (enc.FechaInicio.Value - ahora).TotalDays;
                    if (dias <= 5 && dias > 0)
                        notificaciones.Add($"En {Math.Ceiling(dias)} días tienes turno con {enc.NombreMedico} ({enc.NombreOrganizacion}).");
                    else if (Math.Abs(dias) < 0.5)
                        notificaciones.Add($"Hoy tienes turno con {enc.NombreMedico} ({enc.NombreOrganizacion}).");
                }
            }

            foreach (var vac in vacunas)
            {
                if (vac.FechaAplicacion.HasValue)
                {
                    double dias = (vac.FechaAplicacion.Value - ahora).TotalDays;
                    if (dias <= 5 && dias > 0)
                        notificaciones.Add($"En {Math.Ceiling(dias)} días tienes programada la vacunación de {vac.NombreVacuna}.");
                    else if (Math.Abs(dias) < 0.5)
                        notificaciones.Add($"Hoy debes aplicarte la vacuna {vac.NombreVacuna}.");
                    else if (dias < -1)
                        notificaciones.Add($"Te aplicaste la vacuna {vac.NombreVacuna} el {vac.FechaAplicacion.Value:dd/MM}.");
                }
            }

            foreach (var enc in encuentrosConEstudios)
            {
                var estudios = BD.ObtenerImagenesPorEncuentro(enc.Id);
                foreach (var est in estudios)
                {
                    if (est.Fecha.HasValue)
                    {
                        double dias = (est.Fecha.Value - ahora).TotalDays;
                        if (dias <= 3 && dias > 0)
                            notificaciones.Add($"Tienes un estudio ({est.Modalidad}) programado en {Math.Ceiling(dias)} días.");
                        else if (Math.Abs(dias) < 0.5)
                            notificaciones.Add($"Hoy tienes el estudio {est.Modalidad} de la región {est.Region}.");
                        else if (dias < -1)
                            notificaciones.Add($"El estudio {est.Modalidad} fue realizado el {est.Fecha.Value:dd/MM}.");
                    }
                }
            }

            return notificaciones;
        }
    }


    
    
