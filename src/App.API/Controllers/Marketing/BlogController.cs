namespace App.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/admin/blogs")]
[Authorize]
public class BlogController(
    IBlogService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================

    // GET: api/blogs
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/blogs")]
    public async Task<ActionResult<PagedResult<BlogAllDto>>> GetAllPublicAsync(
        [FromQuery] BlogListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllPublicAsync(query, ct);
        return Ok(result);
    }

    // GET: api/blogs/{slug}
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/blogs/{slug}")]
    public async Task<ActionResult<BlogDto>> GetDetailBySlugAsync(
        [FromRoute] string slug,
        CancellationToken ct)
    {
        var result = await service.GetDetailBySlugAsync(slug, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // GET: api/admin/blogs
    [HttpGet]
    public async Task<ActionResult<PagedResult<BlogDto>>> GetAllAsync(
        [FromQuery] BlogListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    // POST: api/admin/blogs
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateBlogDto dto,
        CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Bloq uğurla yaradıldı." }) : BadRequest(new { success = false, message = "Bloq yaratmaq uğursuz oldu." });
    }

    // PUT: api/admin/blogs/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromForm] UpdateBlogDto dto,
        CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Bloq uğurla yeniləndi." }) : BadRequest(new { success = false, message = "Bloq yeniləmək uğursuz oldu." });
    }

    // PATCH: api/admin/blogs/{id}/status
    [HttpPatch("status/{id:guid}")]
    public async Task<IActionResult> ModifyStatus(
        [FromRoute] Guid id,
        [FromBody] ModifyBlogStatusDto dto,
        CancellationToken ct)
    {
        var ok = await service.ModifyStatusAsync(id, dto.Status, ct);
        return ok ? Ok(new { success = true, message = "Blog statusu yeniləndi." }) : BadRequest(new { success = false, message = "Status dəyişdirmək uğursuz oldu." });
    }

    // DELETE: api/admin/blogs/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Bloq silindi." }) : NotFound(new { success = false, message = "Bloq tapılmadı." });
    }
}
