using Microsoft.AspNetCore.Mvc;

namespace LabUI.ViewComponents
{
    public class CartViewComponent :ViewComponent
    {
        public IViewComponentResult Invoke()
        { return View();}
    }
}
