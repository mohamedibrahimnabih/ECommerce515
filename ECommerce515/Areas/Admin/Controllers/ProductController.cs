using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce515.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var products = _context.Products.Include(e => e.Category).Include(e => e.Brand);

            return View(products.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            var categories = _context.Categories;
            var brands = _context.Brands;

            CategoryWithBrandVM categoryWithBrandVM = new()
            {
                Categories = categories.ToList(),
                Brands = brands.ToList()
            };

            return View(categoryWithBrandVM);
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            _context.Add(product);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int productId)
        {
            var product = _context.Products.Find(productId);

            if(product is not null)
            {
                var categories = _context.Categories;
                var brands = _context.Brands;

                CategoryWithBrandVM categoryWithBrandVM = new()
                {
                    Categories = categories.ToList(),
                    Brands = brands.ToList(),
                    Product = product
                };

                return View(categoryWithBrandVM);
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _context.Update(product);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete([FromRoute] int id)
        {
            var product = _context.Products.Find(id);

            if (product is not null)
            {
                _context.Remove(product);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("NotFoundPage", "Home");
        }
    }
}
