using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;
using System.Text.Encodings.Web;

namespace LabUI.TagHelpers
{
    [HtmlTargetElement("pager")]
    public class PagerTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PagerTagHelper(IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
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
            output.AddClass("d-flex", HtmlEncoder.Default);
            output.AddClass("justify-content-center", HtmlEncoder.Default);

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");

            // Кнопка "Назад"
            var prevLi = CreatePageItem(
                PageCurrent == 1 ? 1 : PageCurrent - 1,
                "<span aria-hidden=\"true\">&laquo;</span>",
                PageCurrent == 1
            );
            ul.InnerHtml.AppendHtml(prevLi);

            // Кнопки с номерами страниц
            for (int i = 1; i <= PageTotal; i++)
            {
                var pageLi = CreatePageItem(i, i.ToString(), i == PageCurrent);
                ul.InnerHtml.AppendHtml(pageLi);
            }

            // Кнопка "Вперед"
            var nextLi = CreatePageItem(
                PageCurrent == PageTotal ? PageTotal : PageCurrent + 1,
                "<span aria-hidden=\"true\">&raquo;</span>",
                PageCurrent == PageTotal
            );
            ul.InnerHtml.AppendHtml(nextLi);

            output.Content.AppendHtml(ul);
        }

        private TagBuilder CreatePageItem(int pageNo, string content, bool isDisabled)
        {
            var li = new TagBuilder("li");
            li.AddCssClass("page-item");
            if (isDisabled && content.Length > 2) // Для стрелок
                li.AddCssClass("disabled");
            else if (pageNo == PageCurrent && content.Length <= 2) // Для номеров
                li.AddCssClass("active");

            var a = new TagBuilder("a");
            a.AddCssClass("page-link");
            a.Attributes["href"] = GenerateUrl(pageNo);
            a.InnerHtml.AppendHtml(content);

            li.InnerHtml.AppendHtml(a);
            return li;
        }

        private string GenerateUrl(int pageNo)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var routeValues = new RouteValueDictionary
            {
                ["pageNo"] = pageNo
            };

            if (!string.IsNullOrEmpty(Category))
            {
                routeValues["category"] = Category;
            }

            return urlHelper.Action(Action, Controller, routeValues);
        }
    }
}