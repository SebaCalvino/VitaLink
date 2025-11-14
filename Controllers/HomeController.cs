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
        return View("LogIn");
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


        private List<Notificacion> GenerarNotificaciones(int idUsuario)
        {
            var notificaciones = new List<Notificacion>();
            var medicaciones = BD.ObtenerMedicacionesPorUsuario(idUsuario);
            var encuentros = BD.ObtenerEncuentrosPorUsuario(idUsuario);
            var vacunas = BD.ObtenerVacunasPorUsuario(idUsuario);
            DateTime ahora = DateTime.Now;

            // Notificaciones de medicamentos
            foreach (var med in medicaciones)
            {
                // Solo procesar medicamentos activos
                if (!med.Estado) continue;

                // Calcular la próxima hora de toma basándome en la frecuencia
                DateTime? proximaToma = CalcularProximaToma(med, ahora);
                
                if (proximaToma.HasValue)
                {
                    TimeSpan diff = proximaToma.Value - ahora;
                    double minutos = diff.TotalMinutes;

                    if (minutos <= 5 && minutos > 0)
                    {
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"¡En {(int)minutos} minutos tomar el {med.Nombre_Comercial}!",
                            Subtitulo = "Ahora",
                            Tipo = "medicamento"
                        });
                    }
                    else if (minutos <= 0 && minutos > -60)
                    {
                        // Si pasó hace menos de una hora, mostrar como "Ahora"
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"¡Es hora de tomar {med.Nombre_Comercial}!",
                            Subtitulo = "Ahora",
                            Tipo = "medicamento"
                        });
                    }
                    else if (minutos <= 10 && minutos > 5)
                    {
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"En {(int)minutos} minutos tomar {med.Nombre_Comercial}",
                            Subtitulo = "Próximamente",
                            Tipo = "medicamento"
                        });
                    }
                }
            }

            // Notificaciones de turnos médicos
            foreach (var enc in encuentros)
            {
                double dias = (enc.FechaInicio - ahora).TotalDays;
                string nombreCompleto = $"{enc.NombreMedico} {enc.ApellidoMedico}".Trim();
                string organizacion = BD.ObtenerNombreOrganizacion(enc.IdOrganizacion) ?? "centro médico";
                
                if (dias <= 1 && dias > 0)
                {
                    notificaciones.Add(new Notificacion
                    {
                        Titulo = $"Turno con {nombreCompleto} - {organizacion}",
                        Subtitulo = dias < 0.5 ? "Hoy" : $"En {Math.Ceiling(dias)} día(s)",
                        Tipo = "turno"
                    });
                }
                else if (dias <= 0 && dias > -1)
                {
                    notificaciones.Add(new Notificacion
                    {
                        Titulo = $"Turno hoy con {nombreCompleto}",
                        Subtitulo = "Hoy",
                        Tipo = "turno"
                    });
                }
            }

            // Notificaciones de vacunas
            foreach (var vac in vacunas)
            {
                double dias = (vac.FechaAplicacion - ahora).TotalDays;
                string nombreVacuna = BD.ObtenerNombreVacuna(vac.IdVacuna) ?? "vacuna";
                
                if (dias <= 1 && dias > 0)
                {
                    notificaciones.Add(new Notificacion
                    {
                        Titulo = $"Vacunación: {nombreVacuna}",
                        Subtitulo = dias < 0.5 ? "Hoy" : $"En {Math.Ceiling(dias)} día(s)",
                        Tipo = "vacuna"
                    });
                }
                else if (dias <= 0 && dias > -1)
                {
                    notificaciones.Add(new Notificacion
                    {
                        Titulo = $"Vacunación: {nombreVacuna}",
                        Subtitulo = "Hoy",
                        Tipo = "vacuna"
                    });
                }
                else if (dias < -1 && dias > -2)
                {
                    // Vacuna aplicada recientemente (últimas 24 horas)
                    int horasAtras = (int)Math.Abs(dias * 24);
                    notificaciones.Add(new Notificacion
                    {
                        Titulo = $"Vacunación: {nombreVacuna}",
                        Subtitulo = horasAtras < 1 ? "Hace menos de 1h" : $"Hace {horasAtras}h",
                        Tipo = "vacuna"
                    });
                }
            }

            // Notificaciones de estudios
            foreach (var enc in encuentros)
            {
                var estudios = BD.ObtenerImagenesPorEncuentro(enc.Id);
                foreach (var est in estudios)
                {
                    double dias = (est.Fecha - ahora).TotalDays;
                    string modalidad = BD.ObtenerModalidadEstudio(est.Id_Modalidad) ?? "Estudio";
                    
                    if (dias <= 1 && dias > 0)
                    {
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"Estudio: {modalidad}",
                            Subtitulo = dias < 0.5 ? "Hoy" : $"En {Math.Ceiling(dias)} día(s)",
                            Tipo = "estudio"
                        });
                    }
                    else if (dias <= 0 && dias > -1)
                    {
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"Estudio: {modalidad}",
                            Subtitulo = "Hoy",
                            Tipo = "estudio"
                        });
                    }
                    else if (dias < -1 && dias > -2)
                    {
                        // Estudio realizado recientemente
                        int horasAtras = (int)Math.Abs(dias * 24);
                        notificaciones.Add(new Notificacion
                        {
                            Titulo = $"Estudio: {modalidad}",
                            Subtitulo = horasAtras < 1 ? "Hace menos de 1h" : $"Hace {horasAtras}h",
                            Tipo = "estudio"
                        });
                    }
                }
            }

            return notificaciones;
        }

        /// <summary>
        /// Calcula la próxima hora de toma de un medicamento basándose en su frecuencia
        /// y la última vez que el usuario lo tomó (HoraProgramada).
        /// </summary>
        private DateTime? CalcularProximaToma(MedicacionesPaciente medicacion, DateTime ahora)
        {
            if (string.IsNullOrEmpty(medicacion.Frecuencia))
                return null;

            // Si no hay HoraProgramada, no podemos calcular la próxima toma
            if (!medicacion.HoraProgramada.HasValue)
                return null;

            // Parsear la frecuencia para obtener el intervalo en horas
            TimeSpan? intervalo = ParsearFrecuencia(medicacion.Frecuencia);
            if (!intervalo.HasValue)
                return null;

            // La última toma es la HoraProgramada
            DateTime ultimaToma = medicacion.HoraProgramada.Value;

            // La próxima toma es la última toma + intervalo
            DateTime proximaToma = ultimaToma.Add(intervalo.Value);

            // Si la próxima toma ya pasó, calcular la siguiente
            // (puede pasar si el usuario no tomó el medicamento a tiempo)
            while (proximaToma < ahora)
            {
                proximaToma = proximaToma.Add(intervalo.Value);
            }

            return proximaToma;
        }

        /// <summary>
        /// Parsea la frecuencia de un medicamento y devuelve el intervalo en horas.
        /// Soporta formatos como: "cada 8 horas", "cada 12 horas", "3 veces al día", etc.
        /// </summary>
        private TimeSpan? ParsearFrecuencia(string frecuencia)
        {
            if (string.IsNullOrEmpty(frecuencia))
                return null;

            frecuencia = frecuencia.ToLower().Trim();

            // Buscar patrones numéricos seguidos de "hora" o "horas"
            var match = System.Text.RegularExpressions.Regex.Match(frecuencia, @"(\d+)\s*(?:hora|horas|hr|hrs)");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int horas))
                {
                    return TimeSpan.FromHours(horas);
                }
            }

            // Buscar "veces al día" o "veces por día"
            match = System.Text.RegularExpressions.Regex.Match(frecuencia, @"(\d+)\s*veces\s*(?:al|por)\s*d[ií]a");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int vecesPorDia))
                {
                    if (vecesPorDia > 0)
                    {
                        // Dividir 24 horas entre las veces por día
                        double horas = 24.0 / vecesPorDia;
                        return TimeSpan.FromHours(horas);
                    }
                }
            }

            // Buscar "cada X" donde X puede ser un número seguido de horas
            match = System.Text.RegularExpressions.Regex.Match(frecuencia, @"cada\s+(\d+)");
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int cada))
                {
                    // Asumir que es en horas si no se especifica
                    return TimeSpan.FromHours(cada);
                }
            }

            // Si no se puede parsear, retornar null
            return null;
        }
    }

            return notificaciones;
        }*/
    }
