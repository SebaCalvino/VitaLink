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

    public IActionResult Perfil()
    {
        return View("Perfil");
    }
    
    public IActionResult Home()
    {
        return View("Home");
    }
}
