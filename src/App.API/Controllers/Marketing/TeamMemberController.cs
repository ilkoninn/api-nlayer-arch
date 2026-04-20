namespace App.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/admin/team-members")]
[Authorize]
public class TeamMemberController(
    ITeamMemberService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================

    // GET: api/team-members
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/team-members")]
    public async Task<ActionResult<PagedResult<TeamMemberDto>>> GetAllAsync(
        [FromQuery] TeamMemberListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // POST: api/admin/team-members
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateTeamMemberDto dto,
        CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Komanda üzvü uğurla əlavə edildi." }) : BadRequest(new { success = false, message = "Komanda üzvü əlavə etmək uğursuz oldu." });
    }

    // PUT: api/admin/team-members/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromForm] UpdateTeamMemberDto dto,
        CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Komanda üzvü uğurla yeniləndi." }) : BadRequest(new { success = false, message = "Komanda üzvü yeniləmək uğursuz oldu." });
    }

    // DELETE: api/admin/team-members/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Komanda üzvü silindi." }) : NotFound(new { success = false, message = "Komanda üzvü tapılmadı." });
    }
}
