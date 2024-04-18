using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RED_BLOG.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")] // This page will be accessible by only Admin
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
