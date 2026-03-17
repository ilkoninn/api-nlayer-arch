using Newtonsoft.Json;

namespace App.API.Controllers;

[ApiController]
[Route("api/admin/projects")]
[Authorize]
public class ProjectController(
    IProjectService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================

    // GET: api/projects
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/projects")]
    public async Task<ActionResult<PagedResult<ProjectAllDto>>> GetAllPublicAsync(
        [FromQuery] ProjectListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllPublicAsync(query, ct);
        return Ok(result);
    }

    // GET: api/projects/{slug}
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/projects/{slug}")]
    public async Task<ActionResult<ProjectDto>> GetDetailBySlugAsync(
        [FromRoute] string slug,
        CancellationToken ct)
    {
        var result = await service.GetDetailBySlugAsync(slug, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // GET: api/admin/projects
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetAllAsync(
        [FromQuery] ProjectListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    // POST: api/admin/projects
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] CreateProjectDto dto,
        CancellationToken ct)
    {
        // Deserialize features from JSON string if provided
        if (!string.IsNullOrWhiteSpace(dto.FeaturesJson))
        {
            var trimmed = dto.FeaturesJson.Trim();
            
            // If single object, wrap in array
            if (trimmed.StartsWith("{"))
            {
                var singleFeature = JsonConvert.DeserializeObject<ProjectFeatureInputDto>(dto.FeaturesJson);
                dto.Features = singleFeature != null ? new List<ProjectFeatureInputDto> { singleFeature } : null;
            }
            // If array, deserialize normally
            else if (trimmed.StartsWith("["))
            {
                dto.Features = JsonConvert.DeserializeObject<List<ProjectFeatureInputDto>>(dto.FeaturesJson);
            }
        }

        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true, message = "Layihə uğurla əlavə edildi." }) : BadRequest(new { success = false, message = "Layihə əlavə etmək uğursuz oldu." });
    }

    // PUT: api/admin/projects/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromForm] UpdateProjectDto dto,
        CancellationToken ct)
    {
        // Deserialize features from JSON string if provided
        if (!string.IsNullOrWhiteSpace(dto.FeaturesJson))
        {
            var trimmed = dto.FeaturesJson.Trim();
            
            // If single object, wrap in array
            if (trimmed.StartsWith("{"))
            {
                var singleFeature = JsonConvert.DeserializeObject<ProjectFeatureInputDto>(dto.FeaturesJson);
                dto.Features = singleFeature != null ? new List<ProjectFeatureInputDto> { singleFeature } : null;
            }
            // If array, deserialize normally
            else if (trimmed.StartsWith("["))
            {
                dto.Features = JsonConvert.DeserializeObject<List<ProjectFeatureInputDto>>(dto.FeaturesJson);
            }
        }

        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true, message = "Layihə uğurla yeniləndi." }) : BadRequest(new { success = false, message = "Layihə yeniləmək uğursuz oldu." });
    }

    // DELETE: api/admin/projects/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true, message = "Layihə silindi." }) : NotFound(new { success = false, message = "Layihə tapılmadı." });
    }
}