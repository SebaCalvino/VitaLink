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




}
     

