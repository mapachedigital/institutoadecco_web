// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using InstitutoAdecco.Models;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IStringLocalizer<IdentityResources> L;
        private readonly IMailUtils _mailUtils;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IStringLocalizer<IdentityResources> localizer,
            IMailUtils mailUtils)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            L = localizer;
            _mailUtils = mailUtils;
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
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, L["Verification email sent. Please check your email."]);
                return Page();
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                Input.Email,
                _mailUtils.GetMailSubject() + L["Confirm your email"],
                L["Please confirm your account by <a href='{0}'>clicking here</a>.", HtmlEncoder.Default.Encode(callbackUrl)]);

            ModelState.AddModelError(string.Empty, L["Verification email sent. Please check your email."]);
            return Page();
        }
    }
}
