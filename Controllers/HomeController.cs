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
        return View("Calendario");
    }
    public IActionResult Crianzas()
    {
        return View("Crianzas");
    }


    public IActionResult Medicamentos()
    {
        Usuario usuario = ObtenerUsuario();
        if (usuario == null) return RedirectToAction("LogIn");

        ViewBag.Medicaciones = BD.ObtenerMedicacionesPorUsuario(usuario.Id);
        return View("Medicamentos");
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
            ViewBag.Medicaciones = BD.ObtenerMedicacionesPorUsuario(usuario.Id);
            ViewBag.Encuentros = BD.ObtenerEncuentrosPorUsuario(usuario.Id);

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

}
     

