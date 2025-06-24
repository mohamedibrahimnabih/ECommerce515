using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce515.ViewModels
{
    public class CategoryWithBrandVM
    {
        public List<SelectListItem> Categories { get; set; } = null!;
        public List<SelectListItem> Brands { get; set; } = null!;
        public Product? Product { get; set; }
    }
}
