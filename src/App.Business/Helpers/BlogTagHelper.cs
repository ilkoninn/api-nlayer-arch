using System.Text.Json;

namespace App.Business.Helpers;

public static class BlogTagHelper
{
    public static string? ToJson(IEnumerable<string>? tags)
    {
        if (tags == null || !tags.Any())
            return null;

        return JsonSerializer.Serialize(tags);
    }

    public static IEnumerable<string>? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<IEnumerable<string>>(json);
        }
        catch
        {
            return null;
        }
    }
}