namespace App.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/admin/contact")]
[Authorize]
public sealed class ContactsController(IContactService service) : ControllerBase
{
	//  ===============================
	//  Public operations
	//  ===============================

	// POST: api/contacts
	[HttpPost]
	[AllowAnonymous]
	[Route("~/api/marketing/contact")]
	public async Task<IActionResult> Create([FromBody] CreateContactDto dto, CancellationToken ct)
	{
		var ok = await service.CreateAsync(dto, ct);
		return ok ? Ok(new { success = true }) : BadRequest(new { success = false });
	}


	//  ===============================
	//  Admin operations
	//  ===============================

	// GET: api/contacts
	[HttpGet]
	public async Task<ActionResult<PagedResult<ContactDto>>> GetAllAsync([FromQuery] ContactListQueryDto query, CancellationToken ct)
	{
		var result = await service.GetAllAsync(query, ct);
		return Ok(result);
	}

	// GET: api/contacts/{id}
	[HttpGet]
	[Route("{id:guid}")]
	public async Task<ActionResult<ContactDto>> GetById([FromRoute] Guid id, CancellationToken ct)
	{
		var contact = await service.GetByIdAsync(id, ct);
		return Ok(contact);
	}

	// PATCH: api/admin/contacts/{id}/status
	[HttpPatch]
	[Route("status/{id:guid}")]
	public async Task<IActionResult> ModifyStatus([FromRoute] Guid id, [FromBody] UpdateContactStatusDto request, CancellationToken ct)
	{
		var changed = await service.ModifyStatusAsync(id, request.Status, ct);
		return changed ? Ok(new { success = true }) : NoContent();
	}

	// DELETE: api/contacts/{id}
	[HttpDelete]
	[Route("{id:guid}")]
	public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
	{
		var ok = await service.RemoveAsync(id, ct);
		return ok ? Ok(new { success = true }) : NotFound();
	}
}
