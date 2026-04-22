using System.Globalization;
using System.Text;

namespace E_commerce.v1.Application.Features.Categories.Services;

public class CategorySlugService : ICategorySlugService
{
    public string GenerateSlug(string categoryName, Guid categoryId)
    {
        var slugBase = Slugify(categoryName);
        var suffix = categoryId.ToString("N", CultureInfo.InvariantCulture)[..8];
        return $"{slugBase}-{suffix}";
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
        // Chặn slug rỗng khi name toàn ký tự đặc biệt.
        return string.IsNullOrEmpty(s) ? "category" : s;
    }
}
