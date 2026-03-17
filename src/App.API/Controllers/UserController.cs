namespace App.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public sealed class UsersController(IUserService userService) : ControllerBase
{
	//  ===============================
	//  Admin operations
	//  ===============================

	// GET: api/admin/users
	[HttpGet]
	public async Task<ActionResult<PagedResult<UserResponseDto>>> GetAll(
		[FromQuery] UserListQueryDto query, CancellationToken ct)
	{
		var result = await userService.GetAllAsync(query, ct);
		return Ok(result);
	}

	// GET: api/admin/users/{id}
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<UserResponseDto>> GetById([FromRoute] Guid id, CancellationToken ct)
	{
		var user = await userService.GetByIdAsync(id, ct);
		return Ok(user);
	}

	// POST: api/admin/users
	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct)
	{
		await userService.CreateAsync(dto, ct);

		return StatusCode(StatusCodes.Status201Created);
	}

	// PUT: api/admin/users/{id}
	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct)
	{
		await userService.UpdateAsync(id, dto, ct);

		return NoContent();
	}


	// PATCH: api/admin/users/{id}/ban
	[HttpPatch("ban/{id:guid}")]
	public async Task<IActionResult> Ban([FromRoute] Guid id, CancellationToken ct)
	{
		await userService.BanAsync(id, ct);

		return NoContent();
	}

	// PATCH: api/admin/users/{id}/unban
	[HttpPatch("unban/{id:guid}")]
	public async Task<IActionResult> Unban([FromRoute] Guid id, CancellationToken ct)
	{
		await userService.UnBanAsync(id, ct);

		return NoContent();
	}


	// DELETE: api/admin/users/{id}   (Hard delete)
	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
	{
		await userService.DeleteAsync(id, ct);

		return NoContent();
	}
}
