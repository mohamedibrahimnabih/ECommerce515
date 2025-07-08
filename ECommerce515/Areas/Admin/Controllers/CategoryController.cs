using ECommerce515.Repositories.IRepositories;
using ECommerce515.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce515.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        //private ApplicationDbContext _context = new();
        //private IRepository<Category> _categoryRepository = new Repository<Category>();
        private ICategoryRepository _categoryRepository;// = new CategoryRepository();

        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin},{SD.Company}")]

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAsync();

            return View(categories);
        }

        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Category());
        }

        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }

            await _categoryRepository.CreateAsync(category);

            //CookieOptions cookie = new()
            //{

            //};
            //Response.Cookies.Append("success-notification", "Add Category Successfully", cookie);
            TempData["success-notification"] = "Add Category Successfully";

            await _categoryRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e=>e.Id == id);

            if(category is not null)
            {
                return View(category);
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        [HttpPost]
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        public async Task<IActionResult> Edit(Category category)
        {
            if(!ModelState.IsValid)
            {
                return View(category);
            }

            _categoryRepository.Edit(category);
            TempData["success-notification"] = "Update Category Successfully";
            await _categoryRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]

        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var category = await _categoryRepository.GetOneAsync(e => e.Id == id);

            if (category is not null)
            {
                _categoryRepository.Delete(category);
                TempData["success-notification"] = "Delete Category Successfully";
                await _categoryRepository.CommitAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("NotFoundPage", "Home");
        }
    }
}
