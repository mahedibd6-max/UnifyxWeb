using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace UnifyxWeb;

// Puts every Razor Page under a language prefix: /en/Pricing, /bn/Pricing ...
public class CultureRouteConvention : IPageRouteModelConvention
{
    public void Apply(PageRouteModel model)
    {
        // Admin pages are English only: /admin/...
        if (model.ViewEnginePath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase)) return;

        foreach (var selector in model.Selectors)
        {
            var template = selector.AttributeRouteModel?.Template ?? "";
            selector.AttributeRouteModel!.Template =
                AttributeRouteModel.CombineTemplates("{culture:regex(^(en|bn)$)}", template);
        }
    }
}
