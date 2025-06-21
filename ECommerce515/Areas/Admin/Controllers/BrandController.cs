using ECommerce515.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce515.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var brands = _context.Brands;

            return View(brands.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Brand());
        }

        [HttpPost]
        public IActionResult Create(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            _context.Add(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit([FromRoute] int id)
        {
            var brand = _context.Brands.Find(id);

            if(brand is not null)
            {
                return View(brand);
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        [HttpPost]
        public IActionResult Edit(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            _context.Update(brand);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete([FromRoute] int id)
        {
            var brand = _context.Brands.Find(id);

            if (brand is not null)
            {
                _context.Remove(brand);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("NotFoundPage", "Home");
        }
    }
}
