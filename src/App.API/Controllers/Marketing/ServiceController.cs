namespace App.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/admin/services")]
[Authorize]
public class ServiceController(
    IServiceService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================

    // GET: api/services
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/services")]
    public async Task<ActionResult<PagedResult<ServiceAllDto>>> GetAllPublicAsync(
        [FromQuery] ServiceListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllPublicAsync(query, ct);
        return Ok(result);
    }

    // GET: api/services/{slug}
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/services/{slug}")]
    public async Task<ActionResult<ServiceDto>> GetDetailBySlugAsync(
        [FromRoute] string slug,
        CancellationToken ct)
    {
        var result = await service.GetDetailBySlugAsync(slug, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // GET: api/admin/services
    [HttpGet]
    public async Task<ActionResult<PagedResult<ServiceDto>>> GetAllAsync(
        [FromQuery] ServiceListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    // POST: api/admin/services
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateServiceDto dto,
        CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Xidmət uğurla əlavə edildi." }) : BadRequest(new { success = false, message = "Xidmət əlavə etmək uğursuz oldu." });
    }

    // PUT: api/admin/services/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromForm] UpdateServiceDto dto,
        CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Xidmət uğurla yeniləndi." }) : BadRequest(new { success = false, message = "Xidmət yeniləmək uğursuz oldu." });
    }

    // DELETE: api/admin/services/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Xidmət silindi." }) : NotFound(new { success = false, message = "Xidmət tapılmadı." });
    }
}
