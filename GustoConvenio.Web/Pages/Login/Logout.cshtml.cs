using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GustoConvenio.Web.Pages.Login;

public class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync("GustoCookie");

        // Força exclusão explícita do cookie
        foreach (var cookie in HttpContext.Request.Cookies.Keys)
            HttpContext.Response.Cookies.Delete(cookie);

        return RedirectToPage("/Login/Index");
    }
}
