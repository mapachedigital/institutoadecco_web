// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstitutoAdecco.Models;

/// <summary>
/// Model for storing a category
/// </summary>
[Index(nameof(Slug), IsUnique = true)]
public class Category
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the category
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The description of the category
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; } = default!;

    /// <summary>
    /// The slug defining the unique URL of the category
    /// </summary>
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = default!;

    /// <summary>
    /// The parent category, if any
    /// </summary>
    [Display(Name = "Parent")]
    public int? ParentId { get; set; }

    /// <summary>
    /// The parent category, if any
    /// </summary>
    [Display(Name = "Parent")]
    public Category? Parent { get; set; }

    /// <summary>
    /// The press releases associated with this category
    /// </summary>
    [Display(Name = "Posts")]
    public List<Post> Posts { get; set; } = [];
}
