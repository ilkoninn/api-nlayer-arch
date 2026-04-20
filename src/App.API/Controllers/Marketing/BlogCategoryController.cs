namespace App.API.Controllers.Marketing;

[ApiController]
[Route("api/marketing/admin/blog-categories")]
[Authorize]
public class BlogCategoryController(
    IBlogCategoryService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================
    
    // GET: api/faqs
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/marketing/blog-categories")]
    public async Task<ActionResult<PagedResult<BlogCategoryDto>>> GetAllAsync(
        [FromQuery] BlogCategoryListQueryDto query,
        CancellationToken ct)
    {
        var result = await service.GetAllAsync(query, ct);
        return Ok(result);
    }

    //  ===============================
    //  Admin operations
    //  ===============================

    // POST: api/faqs
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogCategoryDto dto, CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true }) : BadRequest(new { success = false });
    }


    // PUT: api/faqs
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBlogCategoryDto dto, CancellationToken ct)
    {
        var ok = await service.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { success = true }) : BadRequest(new { success = false });
    }

    // DELETE: api/faqs/{id}
    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var ok = await service.RemoveAsync(id, ct);
        return ok ? Ok(new { success = true }) : NotFound();
    }
}
