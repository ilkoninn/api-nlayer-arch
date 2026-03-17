namespace App.API.Controllers;

[ApiController]
[Route("api/admin/project-categories")]
[Authorize]
public class ProjectCategoryController(
    IProjectCategoryService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================
    
    // GET: api/project-categories
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/project-categories")]
    public async Task<ActionResult<PagedResult<ProjectCategoryDto>>> GetAllAsync(
        [FromQuery] ProjectCategoryListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // POST: api/admin/project-categories
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectCategoryDto dto, CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Kateqoriya uğurla əlavə edildi." }) : BadRequest(new { success = false, message = "Kateqoriya əlavə etmək uğursuz oldu." });
    }

    // PUT: api/admin/project-categories/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProjectCategoryDto dto, CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Kateqoriya uğurla yeniləndi." }) : BadRequest(new { success = false, message = "Kateqoriya yeniləmək uğursuz oldu." });
    }

    // DELETE: api/admin/project-categories/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Kateqoriya silindi." }) : NotFound(new { success = false, message = "Kateqoriya tapılmadı." });
    }
}