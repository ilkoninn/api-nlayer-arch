namespace App.Business.Helpers;

public static class SlugGenerator
{
    /// <summary>
    /// Generates SEO-friendly slug with provided GUID prefix.
    /// </summary>
    public static string GenerateSlug(Guid entityId, string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return entityId.ToString();

        var slug = NameOperation.CharacterRegulatory(input);
        slug = slug.ToLower();

        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        slug = slug.Trim('-');

        var guidPrefix = entityId.ToString().Split('-')[0];
        return $"{guidPrefix}-{slug}";
    }

    /// <summary>
    /// Generates SEO-friendly slug with auto-generated GUID prefix.
    /// </summary>
    public static string GenerateSlug(string input)
    {
        return GenerateSlug(Guid.NewGuid(), input);
    }
}