using System.Text.RegularExpressions;

namespace App.Business.Extensions;

public static class SwaggerTagNameExtensions
{
	private static readonly Regex LowerToUpperBoundary = new("([a-z0-9])([A-Z])", RegexOptions.Compiled);
	private static readonly Regex AcronymToWordBoundary = new("([A-Z]+)([A-Z][a-z])", RegexOptions.Compiled);

	public static string ToSwaggerTagDisplayName(this string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return "API";

		var spaced = AcronymToWordBoundary.Replace(value, "$1 $2");
		spaced = LowerToUpperBoundary.Replace(spaced, "$1 $2");

		return spaced.Trim();
	}
}
