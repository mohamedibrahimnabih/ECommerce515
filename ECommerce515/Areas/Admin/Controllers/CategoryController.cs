using Microsoft.AspNetCore.Mvc;

namespace ECommerce515.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private ApplicationDbContext _context = new();

        public IActionResult Index()
        {
            var categories = _context.Categories;

            return View(categories.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }

            _context.Add(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit([FromRoute] int id)
        {
            var category = _context.Categories.Find(id);

            if(category is not null)
            {
                return View(category);
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }

            _context.Update(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete([FromRoute] int id)
        {
            var category = _context.Categories.Find(id);

            if (category is not null)
            {
                _context.Remove(category);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("NotFoundPage", "Home");
        }
    }
}
