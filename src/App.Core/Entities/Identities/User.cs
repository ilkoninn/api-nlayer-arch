namespace App.Core.Entities.Identities;

public class User : IdentityUser<Guid>
{
	public string? FullName { get; set; }
}
