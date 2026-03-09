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

public class WorkshopController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public WorkshopController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Set<Workshop>()
            .AsNoTracking()
            .Select(x => new WorkshopDto
            {

                WorkshopId = x.WorkshopId,

                Name = x.Name,

                WorkshopType = x.WorkshopType,

                PeopleCount = x.PeopleCount,

            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Set<Workshop>()
            .AsNoTracking()
            .Where(x => x.WorkshopId == id)
            .Select(x => new WorkshopDto
            {

                WorkshopId = x.WorkshopId,

                Name = x.Name,

                WorkshopType = x.WorkshopType,

                PeopleCount = x.PeopleCount,

            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkshopCreateDto dto)
    {
        var entity = new Workshop
        {




            Name = dto.Name,



            WorkshopType = dto.WorkshopType,



            PeopleCount = dto.PeopleCount,


        };

        _db.Set<Workshop>().Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new WorkshopDto
        {

            WorkshopId = entity.WorkshopId,

            Name = entity.Name,

            WorkshopType = entity.WorkshopType,

            PeopleCount = entity.PeopleCount,

        });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<WorkshopUpdateDto> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        var entity = await _db.Set<Workshop>().FirstOrDefaultAsync(x => x.WorkshopId == id);
        if (entity is null) return NotFound();

        var dto = new WorkshopUpdateDto
        {




            Name = entity.Name,



            WorkshopType = entity.WorkshopType,



            PeopleCount = entity.PeopleCount,


        };

        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);





        entity.Name = dto.Name;



        entity.WorkshopType = dto.WorkshopType;



        entity.PeopleCount = dto.PeopleCount;



        await _db.SaveChangesAsync();

        return Ok(new WorkshopDto
        {

            WorkshopId = entity.WorkshopId,

            Name = entity.Name,

            WorkshopType = entity.WorkshopType,

            PeopleCount = entity.PeopleCount,

        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Set<Workshop>().FirstOrDefaultAsync(x => x.WorkshopId == id);
        if (entity is null) return NotFound();

        _db.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
