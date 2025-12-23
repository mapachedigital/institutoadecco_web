// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using InstitutoAdecco.Data;
using InstitutoAdecco.Models;
using MDWidgets;
using MDWidgets.Utils;
using MDWidgets.Utils.ModelAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace InstitutoAdecco.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailUtils _mailUtils;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly IStringLocalizer<IdentityResources> L;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IMailUtils mailUtils,
            IEmailSender emailSender,
            IStringLocalizer<IdentityResources> localizer)
        {
            _mailUtils = mailUtils;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            L = localizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "The '{0}' field is required.")]
            [EmailAddress(ErrorMessage = "The value '{0}' is invalid.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Display(Name = "Company")]
            [StringLength(100, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Required(ErrorMessage = "The '{0}' field is required.")]
            public string Company { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [RequiredCheck(ErrorMessage = "You must accept the privacy policy.")]
            [Display(Name = "I agree with Grupo Adecco privacy policy")]
            public bool AcceptTermsOfService { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = L["Error from external provider: {0}", remoteError];
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = L["Error loading external login information."];
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                // But also update the user information in case it changed in the external service
                var Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var Firstname = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var Lastname = info.Principal.FindFirstValue(ClaimTypes.Surname);

                var user = await _signInManager.UserManager.FindByEmailAsync(Email);

                // This might not be very secure...
                // Consider the scenario: user A with email a@example.com registers normally. User B with email b@example.com registers with Facebook where he/she has the email a@example.com           
                // Unlikely but possible, for example corporate email accounts end up reused
                // if (user.Firstname != Firstname || user.Lastname != Lastname)
                // {
                //    user.Firstname = Firstname;
                //    user.Lastname = Lastname;
                // }
                // Or maybe not, if the user changed his/her name in the external service, it might be a good idea to update it here

                user.LastAccess = DateTime.Now;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Consider this section if you want to allow users to login only if they have a confirmed email.
                // By Samuel Kobelkowsky
                /*
                // The user exists but is not allowed because of email not verified
                if (await _context.UserLogins.FirstOrDefaultAsync(x => x.LoginProvider == info.LoginProvider && x.ProviderKey == info.ProviderKey) != null)
                {
                    ErrorMessage = L["You need to verify your email address.  Please check your inbox."];
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
                */

                // If the user does not have an account, then ask the user to create an account.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = L["Error loading external login information during confirmation."];
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                var Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var Firstname = info.Principal.FindFirstValue(ClaimTypes.GivenName);
                var Lastname = info.Principal.FindFirstValue(ClaimTypes.Surname);

                if (Email != Input.Email)
                {
                    // Something fishy is going on...
                    ErrorMessage = L["The email address provided by the external provider does not match the email you provided."];
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                user.Email = Email;
                user.Firstname = Firstname;
                user.Lastname = Lastname;
                user.Company = Input.Company?.Trim();
                user.AcceptTermsOfService = Input.AcceptTermsOfService;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Globals.RoleCompanyUser);
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, _mailUtils.GetMailSubject() + L["Confirm your email"],
                            L["Please confirm your account by <a href='{0}'>clicking here</a>.", HtmlEncoder.Default.Encode(callbackUrl)]);

                        // If account confirmation is required, we need to show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);

                        user.LastAccess = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private IUserPhoneNumberStore<ApplicationUser> GetPhoneNumberStore()
        {
            if (!_userManager.SupportsUserPhoneNumber)
            {
                throw new NotSupportedException("The default UI requires a user store with phonenumber support.");
            }
            return (IUserPhoneNumberStore<ApplicationUser>)_userStore;
        }
    }
}
