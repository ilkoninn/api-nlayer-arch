namespace App.API.Controllers;

[ApiController]
[Route("api/admin/faqs")]
[Authorize]
public sealed class FaqController(IFaqService service) : ControllerBase
{
    //  ===============================
    //  Public operations
    //  ===============================
    
    // GET: api/faqs
    [HttpGet]
    [AllowAnonymous]
    [Route("~/api/faqs")]
    public async Task<ActionResult<PagedResult<FaqDto>>> GetAllAsync(
        [FromQuery] FaqListQueryDto query,
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
    public async Task<IActionResult> Create([FromBody] CreateFaqDto dto, CancellationToken ct)
    {
        var ok = await service.CreateAsync(dto, ct);
        return ok ? Ok(new { success = true }) : BadRequest(new { success = false });
    }


    // PUT: api/faqs
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFaqDto dto, CancellationToken ct)
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
