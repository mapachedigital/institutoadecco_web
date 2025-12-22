using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InstitutoAdecco.Pages.Error;

/// <summary>
/// Page shown when a NotFound result is sent
/// </summary>
public class _404Model : PageModel
{
    public string? OriginalPathAndQuery { get; set; }

    public void OnGet()
    {
        //  handles non-success HTTP status codes (400-599)
        //  by re-executing the request pipeline with a different path,
        //  allowing for custom error pages while preserving the original URL
        //  and HTTP status code in the browser.
        var statusCodeReExecuteFeature =
            HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        if (statusCodeReExecuteFeature is not null)
        {
            OriginalPathAndQuery = $"{statusCodeReExecuteFeature.OriginalPathBase}"
                                    + $"{statusCodeReExecuteFeature.OriginalPath}"
                                    + $"{statusCodeReExecuteFeature.OriginalQueryString}";

        }
    }
}
