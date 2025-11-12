using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MathFlow.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // Redirect /Index to / for SEO and consistency
        if (HttpContext.Request.Path.Value?.Equals("/Index", StringComparison.OrdinalIgnoreCase) == true)
        {
            return RedirectPermanent("/");
        }

        _logger.LogInformation("Index page accessed");
        return Page();
    }
}
