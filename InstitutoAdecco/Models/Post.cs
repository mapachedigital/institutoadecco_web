// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using MDWidgets.Utils.ModelAttributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstitutoAdecco.Models;

[Index(nameof(Slug), IsUnique = true)]
public class Post
{
    public int Id { get; set; }

    /// <summary>
    /// The title of the post
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = default!;

    /// <summary>
    /// The title of the post
    /// </summary>
    [SanitizeHtml]
    [StringLength(300, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Display(Name = "Summary")]
    public string? Summary { get; set; } = default!;

    /// <summary>
    /// The HTML content of the posting
    /// </summary>
    [SanitizeHtml(false)]
    [DataType(DataType.Html)]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Content")]
    public string Content { get; set; } = default!;

    /// <summary>
    /// The slug, or SEO friendly slug for the post
    /// </summary>
    [RegularExpression(@"[A-Za-z0-9-]*", ErrorMessage = "Only letters, number and dashes allowed.")]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = default!;

    /// <summary>
    /// When the entity was created
    /// </summary>
    [Display(Name = "Created")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [DataType(DataType.DateTime)]
    public DateTime Created { get; set; }

    /// <summary>
    /// When the entity was published
    /// </summary>
    [Display(Name = "Published")]
    [DataType(DataType.DateTime)]
    public DateTime? Published { get; set; }

    /// <summary>
    /// The user who created the entity.
    /// </summary>
    [Display(Name = "Created By")]
    public string? CreatedById { get; set; }

    /// <summary>
    /// The user who created the entity.
    /// </summary>
    [Display(Name = "Created By")]
    public ApplicationUser? CreatedBy { get; set; }

    /// <summary>
    /// When the entity was modified
    /// </summary>
    [Display(Name = "Modified")]
    [DataType(DataType.DateTime)]
    public DateTime? Modified { get; set; }

    /// <summary>
    /// The user who modified the entity.
    /// </summary>
    [Display(Name = "Modified By")]
    public string? ModifiedById { get; set; }

    /// <summary>
    /// The user who modified the entity.
    /// </summary>
    [Display(Name = "Modified By")]
    public ApplicationUser? ModifiedBy { get; set; }

    /// <summary>
    /// The active / draft status of the post
    /// </summary>
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Status")]
    public PostStatus Status { get; set; }

    /// <summary>
    /// A list of categories or tags for the post
    /// </summary>
    [Display(Name = "Categories")]
    public List<Category> Categories { get; set; } = [];

    /// <summary>
    /// A list of categories or tags for the post
    /// </summary>
    [Display(Name = "Tags")]
    public List<Tag> Tags { get; set; } = [];

    /// <summary>
    /// The featured image (thumbnail) for this press note
    /// </summary>
    [Display(Name = "Image")]
    public int? FeaturedImageId { get; set; }

    /// <summary>
    /// The featured image (thumbnail) for this press note
    /// </summary>
    [Display(Name = "Image")]
    public Attachment? FeaturedImage { get; set; }

    /// <summary>
    /// Indicates that the post must be shown first
    /// </summary>
    [Display(Name = "Fixed")]
    public bool Fixed { get; set; } = false;
}

public enum PostStatus
{
    [Display(Name = "Published")]
    Published,

    [Display(Name = "Draft")]
    Draft,

    [Display(Name = "Deleted")]
    Deleted,
}