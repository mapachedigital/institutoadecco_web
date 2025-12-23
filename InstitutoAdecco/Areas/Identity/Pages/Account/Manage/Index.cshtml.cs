// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using InstitutoAdecco.Models;
using MDWidgets;
using MDWidgets.Utils.ModelAttributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace InstitutoAdecco.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IStringLocalizer<IdentityResources> L;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<IdentityResources> localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            L = localizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
            [Phone(ErrorMessage = "The value '{0}' is invalid.")]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [PersonalData]
            [Display(Name = "Firstname")]
            [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
            public string Firstname { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [PersonalData]
            [Display(Name = "Lastname")]
            [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
            public string Lastname { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [PersonalData]
            [Display(Name = "Company")]
            [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
            public string Company { get; set; }

            [Required(ErrorMessage = "The '{0}' field is required.")]
            [PersonalData]
            [RequiredCheck(ErrorMessage = "You must accept the privacy policy.")]
            [Display(Name = "I agree with Grupo Adecco privacy policy")]
            public bool AcceptTermsOfService { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var firstname = user.Firstname;
            var lastname = user.Lastname;
            var company = user.Company;
            var acceptTermsOfService = user.AcceptTermsOfService;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                Firstname = firstname,
                Lastname = lastname,
                Company = company,
                AcceptTermsOfService = acceptTermsOfService,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(L["Unable to load user with ID '{0}'.", _userManager.GetUserId(User)]);
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(L["Unable to load user with ID '{0}'.", _userManager.GetUserId(User)]);
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (user.Firstname != Input.Firstname ||
                user.Lastname != Input.Lastname ||
                user.Company != Input.Company ||
                user.AcceptTermsOfService != Input.AcceptTermsOfService)
            {
                user.Firstname = Input.Firstname.Trim();
                user.Lastname = Input.Lastname.Trim();
                user.Company = Input.Company.Trim();
                user.AcceptTermsOfService = Input.AcceptTermsOfService;

                var userUpdateResult = await _userManager.UpdateAsync(user);
                if (!userUpdateResult.Succeeded)
                {
                    StatusMessage = L["Unexpected error occurred when updating information for user with ID '{0}'.", user.Id];
                    return RedirectToPage();
                }
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber.Trim());
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = L["Unexpected error when trying to set phone number."];
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = L["Your profile has been updated"];
            return RedirectToPage();
        }
    }
}
