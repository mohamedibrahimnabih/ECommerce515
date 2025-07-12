using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Stripe.Checkout;
using System.Threading.Tasks;

namespace ECommerce515.Areas.Customer.Controllers
{
    [Authorize]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public CartController(UserManager<ApplicationUser> userManager, ICartRepository cartRepository, IProductRepository productRepository, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> AddToCart(int productId, int count)
        {
            var user = await _userManager.GetUserAsync(User);

            if(user is null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetOneAsync(e => e.ProductId == productId);
            if (product is null || product.Quantity < count)
            {
                TempData["error-notification"] = "Invalid Count";
                return RedirectToAction("Details", "Home", new { area = "Customer", id = productId });
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is not null)
                cartInDb.Count += count;
            else
            {
                await _cartRepository.CreateAsync(new()
                {
                    ApplicationUserId = user.Id,
                    Count = count,
                    ProductId = productId
                });
            }

            await _cartRepository.CommitAsync();

            TempData["success-notification"] = "Add Product to cart Successfully";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e=>e.Product]);

            ViewBag.totalPrice = carts.Sum(e => e.Product.Price * e.Count);

            return View(carts);
        }

        public async Task<IActionResult> IncrementCount(int productId)
        {

            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            cartInDb.Count++;
            await _cartRepository.CommitAsync();

            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> DecrementCount(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            if(cartInDb.Count > 1)
            {
                cartInDb.Count--;
                await _cartRepository.CommitAsync();
            }

            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            var cartInDb = await _cartRepository.GetOneAsync(e => e.ProductId == productId && e.ApplicationUserId == user.Id);

            if (cartInDb is null)
                return NotFound();

            _cartRepository.Delete(cartInDb);
            await _cartRepository.CommitAsync();

            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user is null)
            {
                return NotFound();
            }

            var carts = await _cartRepository.GetAsync(e => e.ApplicationUserId == user.Id, includes: [e=>e.Product]);

            await _orderRepository.CreateAsync(new()
            {
                ApplicationUserId = user.Id,
                DateTime = DateTime.UtcNow,
                OrderStatus = OrderStatus.pending,
                PaymentMethod = PaymentMethod.Visa,
                TotalPrice = (double)carts.Sum(e => e.Product.Price * e.Count),
            });
            await _orderRepository.CommitAsync();

            var order = (await _orderRepository.GetAsync(e => e.ApplicationUserId == user.Id))
                .OrderBy(e => e.Id)
                .LastOrDefault();

            if (order is null)
                return NotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Success?orderId={order.Id}",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/Customer/Checkout/Cancel?orderId={order.Id}",
            };

            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                            Description = item.Product.Description
                        },
                        UnitAmount = (long)item.Product.Price * 100, // 400.00
                    },
                    Quantity = item.Count,
                });
            }


            var service = new SessionService();
            var session = service.Create(options);
            order.SessionId = session.Id;
            await _orderRepository.CommitAsync();
            return Redirect(session.Url);
        }
    }
}
