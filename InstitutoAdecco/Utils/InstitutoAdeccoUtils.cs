// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using InstitutoAdecco.Data;
using InstitutoAdecco.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace InstitutoAdecco.Utils;

public interface IInstitutoAdeccoUtils
{     /// <summary>
      /// Obtain all the roles that are "inferior" to the current logged in user
      /// </summary>
    Task<List<string>> MySubordinatedRolesAsync();

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    List<string> GetSubordinatedRoles(string myRole);

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    Task<string?> GetCategoryUrlAsync(Category category, IUrlHelper urlHelper);

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    Task<string?> GetPostUrlAsync(Post post, IUrlHelper urlHelper);

    /// <summary>
    /// Get the URL of an attachment
    /// </summary>
    Task<string?> GetAttachmentUrlAsync(Attachment attachment, IUrlHelper urlHelper);
}

public partial class InstitutoAdeccoUtils(UserManager<ApplicationUser> _userManager, IUserUtils _userUtils, ApplicationDbContext _context) : IInstitutoAdeccoUtils
{
    /// <summary>
    /// Obtain all the roles that are "inferior" to the current logged in user
    /// </summary>
    public async Task<List<string>> MySubordinatedRolesAsync()
    {
        var myRole = (await _userManager.GetRolesAsync(await _userUtils.GetUserAsync())).FirstOrDefault();

        if (myRole == null) return [];

        return GetSubordinatedRoles(myRole);
    }

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    public List<string> GetSubordinatedRoles(string myRole)
    {
        // Obtain all the roles
        var allRoles = Globals.Roles.ToList();

        // Admin is not subordinated to anyone
        allRoles.Remove(Globals.RoleAdmin);
        if (myRole == Globals.RoleAdmin) return allRoles;

        // Now the supervisor is not subordinated to anyone remaining in the roles list
        allRoles.Remove(Globals.RoleSupervisor);
        if (myRole == Globals.RoleSupervisor) return allRoles;

        // and so on...
        allRoles.Remove(Globals.RoleCompanyUser);
        return allRoles;
    }

    /// <summary>
    /// Get the URL of a category
    /// </summary>
    public async Task<string?> GetCategoryUrlAsync(Category category, IUrlHelper urlHelper)
    {
        var stack = new Stack<string>();

        stack.Push(category.Slug);

        while (category.ParentId != null)
        {
            category = await _context.Category.FirstAsync(x => x.Id == category.ParentId);
            stack.Push(category.Slug);
        }

        var values = new
        {
            Category = stack.Pop(),
            SubCategory1 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory2 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory3 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory4 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory5 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory6 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory7 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory8 = stack.Count != 0 ? stack.Pop() : null,
            SubCategory9 = stack.Count != 0 ? stack.Pop() : null,
        };

        if (stack.Count > 0) throw new InvalidOperationException("More than 9 levels of recurssion in categories");

        var link = urlHelper.PageLink("/Categoria", values: values);

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Get the URL of a Post
    /// </summary>
    public async Task<string?> GetPostUrlAsync(Post post, IUrlHelper urlHelper)
    {
        var link = urlHelper?.PageLink("/Post", values: new
        {
            Year = post.Created.Year.ToString("0000"),
            Month = post.Created.Month.ToString("00"),
            Day = post.Created.Day.ToString("00"),
            post.Slug,
        });

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Get the URL of an attachment
    /// </summary>
    public async Task<string?> GetAttachmentUrlAsync(Attachment attachment, IUrlHelper urlHelper)
    {
        var regex = GuidUrlMatch();
        var match = regex.Match(attachment.Guid);
        if (!match.Success) return null;

        var link = urlHelper.PageLink("/Uploads", values: new
        {
            Year = match.Groups[1].Value,
            Month = match.Groups[2].Value,
            Path = match.Groups[3].Value,
        });

        if (!Uri.TryCreate(link, UriKind.Absolute, out _)) throw new InvalidOperationException("Generated URL is not absolute");

        return link;
    }

    /// <summary>
    /// Regex to separate the different parts of an attachment Guid /uploads/year/month/file_path
    /// </summary>
    [GeneratedRegex(Globals.GuidRegex)]
    private static partial Regex GuidUrlMatch();
}