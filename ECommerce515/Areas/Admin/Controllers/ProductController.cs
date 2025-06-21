using Microsoft.AspNetCore.Http;
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
        public IActionResult Create(Product product, IFormFile mainImg)
        {
            if (mainImg is not null && mainImg.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImg.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                // Save img in wwwroot
                using (var stream = System.IO.File.Create(filePath))
                {
                    mainImg.CopyTo(stream);
                }

                // Save img in DB
                product.MainImg = fileName;

                // Save product in DB
                _context.Add(product);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return BadRequest();
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
        public IActionResult Edit(Product product, IFormFile? mainImg)
        {
            var productInDB = _context.Products.AsNoTracking().FirstOrDefault(e => e.ProductId == product.ProductId);

            if (productInDB is not null)
            {
                if (mainImg is not null && mainImg.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(mainImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", fileName);

                    // Save img in wwwroot
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        mainImg.CopyTo(stream);
                    }

                    // Delete old img from wwwroot
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", productInDB.MainImg);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Save img in DB
                    product.MainImg = fileName;
                }
                else
                {
                    product.MainImg = productInDB.MainImg;
                }

                // Update img in DB
                _context.Update(product);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
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
