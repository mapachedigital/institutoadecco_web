// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using InstitutoAdecco.Models;
using InstitutoAdecco.Utils;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.Net.Mail;
using System.Text;

namespace InstitutoAdecco.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<IdentityResources> L;
        private readonly IMailUtils _mailUtils;
        private readonly IUserUtils _userUtils;

        public string ReturnUrl { get; set; }

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager,
            IStringLocalizer<IdentityResources> localizer,
            IMailUtils mailUtils,
            IUserUtils userUtils)
        {
            _userManager = userManager;
            L = localizer;
            _mailUtils = mailUtils;
            _userUtils = userUtils;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        public bool Success { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string returnUrl = null)
        {
            // Sanitize the returnUrl
            returnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/")!;

            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(L["Unable to load user with ID '{0}'.", userId]);
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            //StatusMessage = result.Succeeded ? L["Thank you for confirming your email.  Your account  still needs to be approved by an administrator in order to be able to login."] : L["Error confirming your email."];
            StatusMessage = result.Succeeded ? L["Thank you for confirming your email.  You will be redirected in 3 seconds."] : L["Error confirming your email."];
            Success = result.Succeeded;

            /*
            await _mailUtils.SendMailAsync(
                _mailUtils.GetMailSubject() + L["Approval pending"],
                L["Your account  still needs to be approved by an administrator in order to be able to login."],
                user.Email);

            var admins = await _userUtils.GetAllUsersAsync([Globals.RoleAdmin, Globals.RoleSupervisor]);

            foreach (var admin in admins)
            {

                await _mailUtils.SendMailAsync(
                    _mailUtils.GetMailSubject() + L["Approval pending"],
                    L["You have new users needing to be approved in order to use the system."],
                    admin.Email);
            }
            */

            var message = "<h3>" + L["A new user has been registered"] + "</h3>\n" +
                 "<dl>" +
                 "<dt>" + L["Firstname"] + "</dt><dd>" + user.Firstname + "<dd>" +
                 "<dt>" + L["Lastname"] + "</dt><dd>" + user.Lastname + "<dd>" +
                 "<dt>" + L["Company"] + "</dt><dd>" + user.Company + "<dd>" +
                 "<dt>" + L["Mobile Phone"] + "</dt><dd>" + user.PhoneNumber + "<dd>" +
                 "<dt>" + L["Email"] + "</dt><dd>" + user.Email + "<dd>" +
                 "</dl>";

            var admins = await _userUtils.GetAllUsersAsync([Globals.RoleAdmin, Globals.RoleSupervisor]);
            var toAddressList = admins
                .Where(u => !string.IsNullOrEmpty(u.Email))
                .Select(u => new MailAddress(u.Email!, u.FullName))
                .ToList();

            await _mailUtils.SendMailAsync(_mailUtils.GetMailSubject() + L["New User"], message, toAddressList, null, null, null);

            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
