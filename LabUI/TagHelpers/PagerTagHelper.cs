using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace LabUI.TagHelpers
{
    [HtmlTargetElement("pager")]
    public class PagerTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;

        public PagerTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public int PageCurrent { get; set; }
        public int PageTotal { get; set; }
        public string Action { get; set; } = "Index";
        public string Controller { get; set; } = "Product";
        public string? Category { get; set; }
 
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "nav";
            output.Attributes.SetAttribute("aria-label", "Page navigation");

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination justify-content-center");

            // Кнопка "Предыдущая"
            var prevLi = CreatePageItem(PageCurrent == 1 ? PageCurrent : PageCurrent - 1,
                "<span aria-hidden=\"true\">&laquo;</span>", PageCurrent == 1);
            ul.InnerHtml.AppendHtml(prevLi);

            // Нумерация страниц
            for (int i = 1; i <= PageTotal; i++)
            {
                var pageLi = CreatePageItem(i, i.ToString(), i == PageCurrent);
                ul.InnerHtml.AppendHtml(pageLi);
            }

            // Кнопка "Следующая"
            var nextLi = CreatePageItem(PageCurrent == PageTotal ? PageTotal : PageCurrent + 1,
                "<span aria-hidden=\"true\">&raquo;</span>", PageCurrent == PageTotal);
            ul.InnerHtml.AppendHtml(nextLi);

            output.Content.AppendHtml(ul);
        }

        private TagBuilder CreatePageItem(int pageNo, string text, bool isDisabled = false)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");

            if (isDisabled)
                li.AddCssClass("disabled");
            if (text != "&laquo;" && text != "&raquo;" && pageNo == PageCurrent)
                li.AddCssClass("active");

            var a = new TagBuilder("a");
            a.AddCssClass("page-link");

            var urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            var routeValues = new RouteValueDictionary
            {
                { "pageNo", pageNo },
                { "category", Category }
            };

            a.Attributes["href"] = urlHelper.Action(Action, Controller, routeValues);
            a.InnerHtml.AppendHtml(text);

            li.InnerHtml.AppendHtml(a);
            return li;
        }
    }
}