using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Commons.Infrastructure.Repositories;

public static class SearchFunctions
{
    /// <summary>
    /// Maps to PostgreSQL unaccent(); evaluated in memory by providers without it (tests).
    /// </summary>
    public static string Unaccent(string value) => RemoveDiacritics(value);

    /// <summary>
    /// Normalizes a search term the same way the database normalizes the column: unaccented and lower-case.
    /// </summary>
    public static string NormalizeTerm(string value) => RemoveDiacritics(value).ToLowerInvariant();

    public static ModelBuilder UseUnaccent(this ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasDbFunction(typeof(SearchFunctions).GetMethod(nameof(Unaccent), [typeof(string)])!)
            .HasName("unaccent");
        return modelBuilder;
    }

    private static string RemoveDiacritics(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
