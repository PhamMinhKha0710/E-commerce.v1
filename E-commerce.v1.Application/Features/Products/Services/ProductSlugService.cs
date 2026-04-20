using System.Globalization;
using System.Text;

namespace E_commerce.v1.Application.Features.Products.Services;

public class ProductSlugService : IProductSlugService
{
    public string GenerateSku(Guid productId)
    {
        return $"SKU-{productId.ToString("N", CultureInfo.InvariantCulture)[..12].ToUpperInvariant()}";
    }

    public string GenerateSlug(string productName, Guid productId)
    {
        var slugBase = Slugify(productName);
        var productSuffix = productId.ToString("N", CultureInfo.InvariantCulture)[..8];
        return $"{slugBase}-{productSuffix}";
    }

    private static string Slugify(string name)
    {
        var sb = new StringBuilder();
        foreach (var ch in name.Normalize(NormalizationForm.FormD))
        {
            if (char.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(ch))
                sb.Append(char.ToLowerInvariant(ch));
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_')
                sb.Append('-');
        }
        var s = sb.ToString().Trim('-');
        return string.IsNullOrEmpty(s) ? "sp" : s;
    }
}
