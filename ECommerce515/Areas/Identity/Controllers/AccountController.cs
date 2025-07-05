using ECommerce515.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ECommerce515.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if(!ModelState.IsValid)
            {
                return View(registerVM);
            }

            ApplicationUser applicationUser = new()
            {
                UserName = registerVM.UserName,
                Email = registerVM.Email,
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Address = registerVM.Address
            };

            //ApplicationUser applicationUser2 = registerVM.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(applicationUser, registerVM.Password);

            if(result.Succeeded)
            {
                // Send Email
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token = token, userId = applicationUser.Id }, Request.Scheme);
                await _emailSender.SendEmailAsync(registerVM.Email, "Confirm Your Account", $"<h1>Confirm Your Account By Clicking <a href='{link}'>Here</a></h1>");

                // Send msg
                TempData["success-notification"] = "Add Account Successfully, Confirm Your Account!";
                return RedirectToAction(nameof(Index), "Home", new { area = "Customer" });
            }

            foreach (var item in result.Errors)
            {
                ModelState.AddModelError(string.Empty, item.Description);
            }
            //TempData["error-notification"] = String.Join(", ", result.Errors.Select(e=>e.Description));

            return View(registerVM);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if(user is not null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if(result.Succeeded)
                    return View();

                TempData["error-notification"] = String.Join(", ", result.Errors.Select(e=>e.Description));
                return RedirectToAction(nameof(Index), controllerName: "Home", new { area = "Customer" });
            }

            return NotFound();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var user = await _userManager.FindByEmailAsync(loginVM.EmailORUserName) ?? await _userManager.FindByNameAsync(loginVM.EmailORUserName);

            if(user is not null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    TempData["success-notification"] = "Login Successfully";
                    return RedirectToAction(nameof(Index), "Home", new { area = "Customer" });
                }

                if (!user.EmailConfirmed)
                {
                    ModelState.AddModelError(string.Empty, "Confirm Your Account!");
                    return View(loginVM);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Too Many Attempts");
                    return View(loginVM);
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid User Name Or Password");
            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["success-notification"] = "Logout Successfully";
            return RedirectToAction(nameof(Index), "Home", new { area = "Customer" });
        }
    }
}
