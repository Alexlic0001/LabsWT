using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabUI.Areas.Admin.Pages
{
    [Authorize(Roles = "admin")]
    public class DashboardModel : PageModel 
    {
        public void OnGet()
        {
        }
    }
}