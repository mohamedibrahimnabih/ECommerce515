using ECommerce515.Models;
using ECommerce515.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading.Tasks;

namespace ECommerce515.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{SD.SuperAdmin},{SD.Admin}")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;

        //private ApplicationDbContext _context = new();

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAsync(includes: [e => e.Category, e => e.Brand]);

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAsync();
            var brands = await _categoryRepository.GetAsync();

            CategoryWithBrandVM categoryWithBrandVM = new()
            {
                Categories = categories.Select(e=> new SelectListItem()
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList(),
                Brands = brands.Select(e => new SelectListItem()
                {
                    Text = e.Name,
                    Value = e.Id.ToString()
                }).ToList(),
                Product = new()
            };

            return View(categoryWithBrandVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile mainImg)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepository.GetAsync();
                var brands = await _categoryRepository.GetAsync();

                CategoryWithBrandVM categoryWithBrandVM = new()
                {
                    Categories = categories.Select(e => new SelectListItem()
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    }).ToList(),
                    Brands = brands.Select(e => new SelectListItem()
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    }).ToList(),
                    Product = new()
                };

                return View(categoryWithBrandVM);
            }

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
                await _productRepository.CreateAsync(product);
                await _categoryRepository.CommitAsync();

                return RedirectToAction(nameof(Index));
            }

            return BadRequest();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int productId)
        {
            var product = await _productRepository.GetOneAsync(e => e.ProductId == productId);

            if (product is not null)
            {
                var categories = await _categoryRepository.GetAsync();
                var brands = await _categoryRepository.GetAsync();

                CategoryWithBrandVM categoryWithBrandVM = new()
                {
                    Categories = categories.Select(e => new SelectListItem()
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    }).ToList(),
                    Brands = brands.Select(e => new SelectListItem()
                    {
                        Text = e.Name,
                        Value = e.Id.ToString()
                    }).ToList(),
                    Product = product
                };

                return View(categoryWithBrandVM);
            }

            return RedirectToAction("NotFoundPage", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile? mainImg)
        {
            var productInDB = await _productRepository.GetOneAsync(e => e.ProductId == product.ProductId, tracked: false);

            if (productInDB is not null)
            {
                if (!ModelState.IsValid)
                {
                    var categories = await _categoryRepository.GetAsync();
                    var brands = await _categoryRepository.GetAsync();
                    product.MainImg = productInDB.MainImg;

                    CategoryWithBrandVM categoryWithBrandVM = new()
                    {
                        Categories = categories.Select(e => new SelectListItem()
                        {
                            Text = e.Name,
                            Value = e.Id.ToString()
                        }).ToList(),
                        Brands = brands.Select(e => new SelectListItem()
                        {
                            Text = e.Name,
                            Value = e.Id.ToString()
                        }).ToList(),
                        Product = product
                    };

                    return View(categoryWithBrandVM);
                }

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
                _productRepository.Edit(product);
                await _categoryRepository.CommitAsync();

                return RedirectToAction(nameof(Index));
            }

            return NotFound();
        }

        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var product = await _productRepository.GetOneAsync(e => e.ProductId == id);

            if (product is not null)
            {
                // Delete old img from wwwroot
                var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", product.MainImg);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                _productRepository.Delete(product);
                await _categoryRepository.CommitAsync();

                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction("NotFoundPage", "Home");
        }
    }
}
