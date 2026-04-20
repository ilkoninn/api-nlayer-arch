namespace App.Business.Helpers;

public static class JwtTokenGenerator
{
	public static string GenerateToken(Guid userId, IEnumerable<string> roles, IConfiguration configuration)
	{
		var secretKey = configuration["JwtSettings:SecretKey"] ??
			throw new InvalidOperationException("JWT Secret Key tənzimlənməyib.");
		var issuer = configuration["JwtSettings:Issuer"] ??
			throw new InvalidOperationException("JWT Issuer tənzimlənməyib.");
		var audience = configuration["JwtSettings:Audience"] ??
			throw new InvalidOperationException("JWT Audience tənzimlənməyib.");
		var expiration = DateTime.UtcNow.AddDays(1);
		var claims = new List<Claim> { new("uid", userId.ToString()) };
		foreach (var r in roles.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal))
			claims.Add(new Claim("role", r.Trim()));

		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.UTF8.GetBytes(secretKey);

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(claims),
			Expires = expiration,
			Issuer = issuer,
			Audience = audience,
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
				SecurityAlgorithms.HmacSha256Signature)
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		var tokenString = tokenHandler.WriteToken(token);

		return tokenString;
	}

	public static DateTime GetTokenExpiration(string token, IConfiguration configuration)
	{
		if (string.IsNullOrWhiteSpace(token))
			throw new ArgumentException("Token boş ola bilməz.", nameof(token));

		var secretKey = configuration["JwtSettings:SecretKey"] ??
			throw new InvalidOperationException("JWT Secret Key tənzimlənməyib.");

		var issuer = configuration["JwtSettings:Issuer"] ??
			throw new InvalidOperationException("JWT Issuer tənzimlənməyib.");

		var audience = configuration["JwtSettings:Audience"] ??
			throw new InvalidOperationException("JWT Audience tənzimlənməyib.");

		var tokenHandler = new JwtSecurityTokenHandler();

		var key = Encoding.UTF8.GetBytes(secretKey);


		tokenHandler.ValidateToken(token, new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(key),
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = false
		}, out SecurityToken validatedToken);

		if (validatedToken is JwtSecurityToken jwtToken)
			return jwtToken.ValidTo;

		throw new Exception("Invalid token format.");
	}
}
