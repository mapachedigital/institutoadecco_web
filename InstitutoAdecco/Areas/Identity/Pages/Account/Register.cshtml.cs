// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using InstitutoAdecco.Models;
using InstitutoAdecco.Utils;
using MDWidgets;
using MDWidgets.Utils;
using MDWidgets.Utils.ModelAttributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;

namespace InstitutoAdecco.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserUtils _userUtils;
        private readonly IMailUtils _mailUtils;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<IdentityResources> L;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            IUserUtils userUtils,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IStringLocalizer<IdentityResources> localizer,
            IMailUtils mailUtils)
        {
            _userManager = userManager;
            _userStore = userStore;
            _userUtils = userUtils;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _mailUtils = mailUtils;
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
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Display(Name = "Firstname")]
            [StringLength(100, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Required(ErrorMessage = "The '{0}' field is required.")]
            public string Firstname { get; set; }

            [Display(Name = "Lastname")]
            [StringLength(100, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Required(ErrorMessage = "The '{0}' field is required.")]
            public string Lastname { get; set; }

            [Display(Name = "Company")]
            [StringLength(100, ErrorMessage = "The '{0}' field must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
            [Required(ErrorMessage = "The '{0}' field is required.")]
            public string Company { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [RequiredCheck(ErrorMessage = "You must accept the privacy policy.")]
            [Display(Name = "I agree with Grupo Adecco privacy policy")]
            public bool AcceptTermsOfService { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [RequiredCheck(ErrorMessage = "You must accept the use of the company logo on the platform.")]
            [Display(Name = "I agree to the use of the company logo on the platform")]
            public bool AcceptUsageOfLogo { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "The '{0}' field is required.")]
            [EmailAddress(ErrorMessage = "The value '{0}' is invalid.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "The '{0}' field is required.")]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Action("Index", "Home", new { area = "Admin" });

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Firstname = Input.Firstname?.Trim();
                user.Lastname = Input.Lastname?.Trim();
                user.Company = Input.Company?.Trim();
                user.AcceptTermsOfService = Input.AcceptTermsOfService;
                user.EmailConfirmed = false;
                user.PhoneNumberConfirmed = false;
                // Comment this to allow admin approval process
                user.Approved = true;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _userUtils.GetEmailStore().SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    await _userManager.AddToRoleAsync(user, Globals.RoleCompanyUser);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, _mailUtils.GetMailSubject() + L["Confirm your email"],
                        L["Please confirm your account by <a href='{0}'>clicking here</a>.", HtmlEncoder.Default.Encode(callbackUrl)]);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

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

            // If we got this far, something failed, redisplay form
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
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
