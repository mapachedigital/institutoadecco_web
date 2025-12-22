using MDWidgets.Utils.ModelAttributes;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace InstitutoAdecco.Models;

/// <summary>
/// Model for storing a tag
/// </summary>
[Index(nameof(Slug), IsUnique = true)]
public class Tag
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the tag
    /// </summary>
    [SanitizeHtml]
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The slug defining the unique URL of the category
    /// </summary>
    [StringLength(200, ErrorMessage = "The '{0}' field must have a maximum of {1} characters.")]
    [Required(ErrorMessage = "The '{0}' field is required.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = default!;

    /// <summary>
    /// The number of posts associated with this tag
    /// </summary>
    [Display(Name = "Count")]
    public int Count { get; set; } = 0;

    /// <summary>
    /// The parent tag, if any
    /// </summary>
    [Display(Name = "Parent")]
    public int? ParentId { get; set; }

    /// <summary>
    /// The parent tag, if any
    /// </summary>
    [Display(Name = "Parent")]
    public Tag? Parent { get; set; }

    /// <summary>
    /// The press releases associated with this tag
    /// </summary>
    [Display(Name = "Posts")]
    public List<Post> Posts { get; set; } = [];
}
