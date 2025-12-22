// Copyright (c) 2021, Mapache Digital
// Version: 1.4.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InstitutoAdecco.Models;

/// <summary>
/// Additional fields for the Identity User model
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The firstname of the user.
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [PersonalData]
    [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Firstname")]
    public string Firstname { get; set; } = default!;

    /// <summary>
    /// The lastname of the user.
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [PersonalData]
    [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Lastname")]
    public string Lastname { get; set; } = default!;

    /// <summary>
    /// The company of the user.
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [PersonalData]
    [StringLength(80, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Company")]
    public string Company { get; set; } = default!;

    [Required(ErrorMessage = "The '{0}' field is required.")]
    [PersonalData]
    [RequiredCheck(ErrorMessage = "You must accept the privacy policy.")]
    [Display(Name = "I agree with Grupo Adecco privacy policy")]
    public bool AcceptTermsOfService { get; set; }

    /// <summary>
    /// The locale of the user for the UI and number formatting. Example: es-MX.
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [PersonalData]
    [StringLength(16, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Language")]
    public string Language { get; set; } = default!;

    /// <summary>
    /// The date and time that the user was seen around
    /// </summary>
    [Display(Name = "Last Access")]
    [DataType(DataType.DateTime)]
    public DateTime LastAccess { get; set; }

    /// <summary>
    /// Whether the user has been approved by an administrator
    /// </summary>
    [Display(Name = "Approved")]
    public bool Approved { get; set; }

    /// <summary>
    /// Calculated value of the full name of the user.
    /// </summary>
    [Display(Name = "Name")]
    public string FullName
    {
        get
        {
            return (Firstname + " " + Lastname).Trim();
        }
    }
}
