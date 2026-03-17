namespace App.API.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize]
public sealed class SettingsController(ISettingService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================

    // GET: api/settings
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/settings")]
    public async Task<ActionResult<PagedResult<SettingDto>>> GetAllPublicAsync(
        [FromQuery] SettingListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllPublicAsync(query, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // POST: api/admin/settings
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateSettingDto dto,
        CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Tənzimləmə əlavə edildi." }) : BadRequest(new { success = false, message = "Tənzimləmə əlavə etmə uğursuz oldu." });
    }

    // PUT: api/admin/settings/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromForm] UpdateSettingDto dto,
        CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Tənzimləmə yeniləndi." }) : BadRequest(new { success = false, message = "Tənzimləmə yeniləmə uğursuz oldu." });
    }

    // DELETE: api/admin/settings/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Tənzimləmə silindi." }) : NotFound(new { success = false, message = "Tənzimləmə tapılmadı." });
    }
}
