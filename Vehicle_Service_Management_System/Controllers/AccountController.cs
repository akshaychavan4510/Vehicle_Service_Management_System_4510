using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.ViewModels.Account;
using Vehicle_Service_Management_System.Infrastructure.Identity;

namespace Vehicle_Service_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "This account has been locked out. Please try again later.");
            else
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (await _userManager.IsInRoleAsync(user, model.Role) == false)
                    await _userManager.AddToRoleAsync(user, model.Role);

                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["Success"] = "Account created successfully.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // Logout always lands the user on a page they can actually see while
        // signed out. A returnUrl is only honored if it's local AND does not
        // point back into an authenticated-only area (like Home) — otherwise
        // we fall back to the Login page. This is what fixes the "logout
        // sends me to the wrong page" issue: previously it always redirected
        // to Home/Index, which requires auth, so you'd immediately get
        // bounced somewhere unexpected right after signing out.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl)
                && !returnUrl.TrimStart('~').StartsWith("/Home", StringComparison.OrdinalIgnoreCase))
            {
                return Redirect(returnUrl);
            }

            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
