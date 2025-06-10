using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ECommerce515.Models;
using ECommerce515.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ECommerce515.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private ApplicationDbContext _context = new();

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        var result = _context.Products.Include(e => e.Category).Include(e => e.Brand);

        // Filter
        // Join
        // Pagination
        // Others..

        return View(result.ToList());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public ViewResult Welcome()
    {
        int age = 21;
        string name = "Mohamed";
        List<string> skills = ["C++", "C#", "SQL"];

        var result = new PersonalInfoVM { Age = age, Name = name, Skills = skills };

        return View(result);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
