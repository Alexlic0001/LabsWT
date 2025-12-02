using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace LabUI.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "img-action, img-controller")]
    public class ImageTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlHelperFactory;

        public ImageTagHelper(IUrlHelperFactory urlHelperFactory)
        {
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public string ImgAction { get; set; }
        public string ImgController { get; set; }
        public string? ImageName { get; set; } // Добавьте это свойство

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var imageUrl = urlHelper.Action(ImgAction, ImgController, new { imageName = ImageName });

            output.Attributes.SetAttribute("src", imageUrl);
        }
    }
}