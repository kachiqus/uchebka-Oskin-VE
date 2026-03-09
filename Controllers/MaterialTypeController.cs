using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;

using Microsoft.AspNetCore.Authorization;


using oskin_site.Data;
using oskin_site.Models;
using oskin_site.Dtos;

namespace oskin_site.Controllers;

[ApiController]
[Route("api/[controller]")]

[Authorize]

public class MaterialTypeController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public MaterialTypeController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Set<MaterialType>()
            .AsNoTracking()
            .Select(x => new MaterialTypeDto
            {

                MaterialTypeId = x.MaterialTypeId,

                Name = x.Name,

                LossPercent = x.LossPercent,

            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Set<MaterialType>()
            .AsNoTracking()
            .Where(x => x.MaterialTypeId == id)
            .Select(x => new MaterialTypeDto
            {

                MaterialTypeId = x.MaterialTypeId,

                Name = x.Name,

                LossPercent = x.LossPercent,

            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaterialTypeCreateDto dto)
    {
        var entity = new MaterialType
        {




            Name = dto.Name,



            LossPercent = dto.LossPercent,


        };

        _db.Set<MaterialType>().Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new MaterialTypeDto
        {

            MaterialTypeId = entity.MaterialTypeId,

            Name = entity.Name,

            LossPercent = entity.LossPercent,

        });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<MaterialTypeUpdateDto> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        var entity = await _db.Set<MaterialType>().FirstOrDefaultAsync(x => x.MaterialTypeId == id);
        if (entity is null) return NotFound();

        var dto = new MaterialTypeUpdateDto
        {




            Name = entity.Name,



            LossPercent = entity.LossPercent,


        };

        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);





        entity.Name = dto.Name;



        entity.LossPercent = dto.LossPercent;



        await _db.SaveChangesAsync();

        return Ok(new MaterialTypeDto
        {

            MaterialTypeId = entity.MaterialTypeId,

            Name = entity.Name,

            LossPercent = entity.LossPercent,

        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Set<MaterialType>().FirstOrDefaultAsync(x => x.MaterialTypeId == id);
        if (entity is null) return NotFound();

        _db.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
