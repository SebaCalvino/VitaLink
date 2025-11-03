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

    public IActionResult LogIn()
    {
        return View("LogIn");
    }

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

    public IActionResult Perfil(int idUsuario)
    {
        ViewBag.Usuario = BD.ObtenerUsuarioPorId(idUsuario);
        ViewBag.Alergias = BD.ObtenerAlergiasPorUsuario(idUsuario);
        ViewbAG.Diagnosticos = BD.ObtenerDiagnosticosPorUsuario(idUsuario);
        return View("Perfil");
    }
    
    public IActionResult Home(int idUsuario)
    {
        var usuario = BD.ObtenerUsuarioPorId(idUsuario);
        var medicaciones = BD.ObtenerMedicacionesPorUsuario(idUsuario);
        var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);

        ViewBag.Usuario = usuario;
        ViewBag.Medicaciones = medicaciones;
        ViewBag.Encuentros = encuentros;
        ViewBag.Notificaciones = GenerarNotificaciones(usuario.Id);

        return View("Home");
    }
    private List<string> GenerarNotificaciones(int idUsuario)
    {
        var notificaciones = new List<string>();
        var medicaciones = BD.ObtenerMedicacionesPorUsuario(idUsuario);
        var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
        var vacunas = BD.ObtenerVacunasPorUsuario(idUsuario);
        var encuentrosConEstudios = BD.ObtenerEncuentrosPorUsuario(idUsuario);

        DateTime ahora = DateTime.Now;

        // MEDICACIONES
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

        // TURNOS / ENCUENTROS
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

        // VACUNAS
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

        // ESTUDIOS (Imagenes o Documentos Clínicos)
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
    public IActionResult HistorialMedico(int idUsuario )
    {
            // Usuario base
        var usuario = BD.ObtenerUsuarioPorId(idUsuario);
        ViewBag.Usuario = usuario;


            // Diagnósticos / enfermedades
        var diagnosticos = BD.ObtenerDiagnosticosPorUsuario(idUsuario);
        ViewBag.Diagnosticos = diagnosticos;


            // Encuentros y documentos clínicos asociados
        var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
        var documentos = encuentros
                .SelectMany(e => BD.ObtenerDocumentosPorEncuentro(e.Id))
                .ToList();
        ViewBag.Documentos = documentos;


            // Vacunas
        ViewBag.Vacunas = BD.ObtenerVacunasPorUsuario(idUsuario);


            // Antecedentes familiares (a definir en BD). Por ahora, lista vacía para no romper la vista
        ViewBag.AntecedentesFamiliares = new System.Collections.Generic.List<object>();


        return View("HistorialMedico");
    }
    [HttpPost]
    public IActionResult CambiarContrasena(string contrasenaActual, string contrasenaNueva)
    {
        Usuario usuario = BD.ObtenerUsuarioPorId(idUsuario);
        bool resultado = usuario.CambiarContraseña(contrasenaActual, contrasenaNueva);

        if (!resultado && usuario.Intentos >= 5)
        {
            return Json(new { success = false, captcha = true });
        }

        return Json(new { success = resultado, captcha = false });
    }





    
    }
