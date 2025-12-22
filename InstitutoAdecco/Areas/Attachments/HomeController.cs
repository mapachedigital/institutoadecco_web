// Copyright (c) 2021, Mapache Digital
// Version: 1.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using InstitutoAdecco.Data;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

namespace InstitutoAdecco.Areas.Attachments;

[Area("Attachments")]
[AllowAnonymous]
public class HomeController(ApplicationDbContext _context,
            IStorageUtils _storageUtils,
            IWebHostEnvironment _hostEnvironment) : Controller
{
    // GET: Assets/Image/5
    /// <summary>
    /// Return the an attachment with the given ID
    /// </summary>
    public async Task<IActionResult> File(int? id, Models.ContentDisposition? disposition)
    {
        if (id == null)
        {
            return NotFound();
        }

        var attachment = await _context.Attachment
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attachment == null)
        {
            return NotFound();
        }

        var upload = await _storageUtils.GetFileAsync(attachment.File, attachment.Container, attachment.Location);
        if (upload == null)
        {
            return NotFound();
        }

        using var stream = upload.Value.Item1;

        var mimeType = upload.Value.Item2;
        var data = stream.ToArray();

        if (disposition == Models.ContentDisposition.Attachment)
        {
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("attachment") { FileName = attachment.OriginalFilename, }.ToString();
            return File(data, mimeType, attachment.OriginalFilename);
        }
        else
        {
            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline") { FileName = attachment.OriginalFilename, }.ToString();
            return File(data, mimeType);
        }
    }

    // GET: Assets/Thumb/5
    /// <summary>
    /// Return the attachment thumbnail for the file with the given ID. If no thumbnail found, return a placeholder image.
    /// </summary>
    public async Task<IActionResult> Thumb(int? id)
    {
        if (id == null)
        {
            return RedirectToAction(nameof(Placeholder));
        }

        var attachment = await _context.Attachment
            .FirstOrDefaultAsync(m => m.Id == id);

        if (attachment == null || attachment.ThumbFile == null || attachment.ThumbContainer == null)
        {
            // No thumb but the attachment is an image, so we redirect to the full image
            if (attachment?.MimeType.StartsWith("image/") == true)
                return RedirectToAction(nameof(File), new { Id = id });

            return RedirectToAction(nameof(Placeholder));
        }

        var upload = await _storageUtils.GetFileAsync(attachment.ThumbFile, attachment.ThumbContainer, attachment.Location);
        if (upload == null)
        {
            return RedirectToAction(nameof(Placeholder));
        }

        using var stream = upload.Value.Item1;

        var mimeType = upload.Value.Item2;
        var data = stream.ToArray();
        return File(data, mimeType);
    }

    // GET: Assets/Thumb/5
    /// <summary>
    /// Return a generic placeholder.
    /// </summary>
    public IActionResult Placeholder()
    {
        var path = Path.Combine(_hostEnvironment.WebRootPath, "images", "image-404.png");
        var placeholder = System.IO.File.OpenRead(path);
        var placeholderMimeType = FileUtils.GetMIMEType(path);
        return File(placeholder, placeholderMimeType);
    }
}
