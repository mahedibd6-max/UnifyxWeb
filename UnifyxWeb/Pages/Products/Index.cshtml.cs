using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifyxWeb.Data;

namespace UnifyxWeb.Pages.Products;

// /en/Products → all products;  /en/Products/<slug> → one product.
public class IndexModel : PageModel
{
    private readonly ProductMenu _menu;
    public IndexModel(ProductMenu menu) => _menu = menu;

    public List<Product> Items { get; set; } = new();
    public Product? One { get; set; }

    public IActionResult OnGet(string? slug)
    {
        Items = _menu.Items;
        if (string.IsNullOrEmpty(slug)) return Page();
        One = Items.FirstOrDefault(x => string.Equals(x.Slug, slug, StringComparison.OrdinalIgnoreCase));
        return One == null ? NotFound() : Page();
    }
}
