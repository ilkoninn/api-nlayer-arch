using System.Globalization;
using System.Text;

namespace App.Business.Helpers;

/// <summary>
/// Portal <see cref="User.UserName"/> üçün: Azərbaycan latın əlifbası → ASCII latın,
/// ad/soyad ayrılanda <c>tamAd.tamSoyad</c> (nöqtə ilə; məs. Nərgiz Məmmədova → <c>nargiz.mammadova</c>),
/// unikallıq suffix serverdə <c>-1</c> və s. <c>ə</c> → <c>a</c> (portal üçün).
/// </summary>
public static class StudentPortalUserName
{
	/// <summary>
	/// Yeni tələbə üçün portal istifadəçi adının əsas hissəsi (kiçik hərflə, nöqtə ilə).
	/// </summary>
	public static string BuildStem(string? firstName, string? lastName, string? fullName, string displayName)
	{
		var fromNames = TryBuildStemFromNames(firstName, lastName, fullName);
		if (!string.IsNullOrEmpty(fromNames))
			return fromNames;

		return FallbackStemFromDisplayName(displayName);
	}

	private static string? TryBuildStemFromNames(string? firstName, string? lastName, string? fullName)
	{
		var fn = Trim(firstName);
		var ln = Trim(lastName);
		if (fn.Length > 0 && ln.Length > 0)
		{
			var s = BuildFullFirstDotFullLast(fn, ln);
			return s.Length > 0 ? s : null;
		}

		var full = Trim(fullName);
		if (full.Length == 0)
			return null;

		var parts = full.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length >= 2)
		{
			var s = BuildFullFirstDotFullLast(parts[0], parts[^1]);
			return s.Length > 0 ? s : null;
		}

		var single = TransliterateToAscii(parts[0]);
		var seg = KeepAsciiLettersDigits(single);
		return seg.Length > 0 ? seg.ToLowerInvariant() : null;
	}

	private static string BuildFullFirstDotFullLast(string firstToken, string lastToken)
	{
		var firstPart = KeepAsciiLettersDigits(TransliterateToAscii(firstToken));
		var lastPart = KeepAsciiLettersDigits(TransliterateToAscii(lastToken));
		if (firstPart.Length == 0 || lastPart.Length == 0)
			return string.Empty;
		return $"{firstPart}.{lastPart}".ToLowerInvariant();
	}

	private static string FallbackStemFromDisplayName(string displayName)
	{
		var trimmed = displayName.Trim();
		if (trimmed.Length == 0)
			return "student";

		var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length >= 2)
		{
			var stem = BuildFullFirstDotFullLast(parts[0], parts[^1]);
			if (stem.Length > 0)
				return stem;
		}

		var pieces = new List<string>();
		foreach (var p in parts)
		{
			var t = TransliterateToAscii(p);
			var seg = KeepAsciiLettersDigits(t);
			if (seg.Length > 0)
				pieces.Add(seg);
		}

		if (pieces.Count == 0)
			return "student";
		return string.Join(".", pieces).ToLowerInvariant();
	}

	private static string KeepAsciiLettersDigits(string s)
	{
		var sb = new StringBuilder(s.Length);
		foreach (var c in s)
		{
			if (char.IsAsciiLetter(c) || char.IsAsciiDigit(c))
				sb.Append(c);
		}
		return sb.ToString();
	}

	private static string Trim(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();

	/// <summary>
	/// Azərbaycan latın hərfləri və ümumi dia kritik işarələri ASCII latına yaxınlaşdırır.
	/// </summary>
	public static string TransliterateToAscii(string input)
	{
		if (string.IsNullOrEmpty(input))
			return "";

		var sb = new StringBuilder(input.Length);
		foreach (var c in input.Normalize(NormalizationForm.FormD))
		{
			if (char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
				continue;

			if (TryMapAzerbaijaniLatin(c, out var mapped))
			{
				sb.Append(char.ToLowerInvariant(mapped));
				continue;
			}

			if (char.IsAsciiLetter(c) || char.IsAsciiDigit(c))
				sb.Append(char.ToLowerInvariant(c));
			else if (c is ' ' or '\t')
				sb.Append(' ');
		}

		return sb.ToString();
	}

	private static bool TryMapAzerbaijaniLatin(char ch, out char mapped)
	{
		switch (ch)
		{
			case 'ə':
			case 'Ə':
				mapped = 'a';
				return true;
			case 'ı':
				mapped = 'i';
				return true;
			case 'İ':
				mapped = 'i';
				return true;
			case 'ö':
			case 'Ö':
				mapped = 'o';
				return true;
			case 'ü':
			case 'Ü':
				mapped = 'u';
				return true;
			case 'ğ':
			case 'Ğ':
				mapped = 'g';
				return true;
			case 'ş':
			case 'Ş':
				mapped = 's';
				return true;
			case 'ç':
			case 'Ç':
				mapped = 'c';
				return true;
			default:
				mapped = default;
				return false;
		}
	}
}
