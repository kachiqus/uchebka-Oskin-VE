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

public class ProductTypeController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductTypeController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Set<ProductType>()
            .AsNoTracking()
            .Select(x => new ProductTypeDto
            {

                ProductTypeId = x.ProductTypeId,

                Name = x.Name,

                Coefficient = x.Coefficient,

            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Set<ProductType>()
            .AsNoTracking()
            .Where(x => x.ProductTypeId == id)
            .Select(x => new ProductTypeDto
            {

                ProductTypeId = x.ProductTypeId,

                Name = x.Name,

                Coefficient = x.Coefficient,

            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductTypeCreateDto dto)
    {
        var entity = new ProductType
        {




            Name = dto.Name,



            Coefficient = dto.Coefficient,


        };

        _db.Set<ProductType>().Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new ProductTypeDto
        {

            ProductTypeId = entity.ProductTypeId,

            Name = entity.Name,

            Coefficient = entity.Coefficient,

        });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<ProductTypeUpdateDto> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        var entity = await _db.Set<ProductType>().FirstOrDefaultAsync(x => x.ProductTypeId == id);
        if (entity is null) return NotFound();

        var dto = new ProductTypeUpdateDto
        {




            Name = entity.Name,



            Coefficient = entity.Coefficient,


        };

        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);





        entity.Name = dto.Name;



        entity.Coefficient = dto.Coefficient;



        await _db.SaveChangesAsync();

        return Ok(new ProductTypeDto
        {

            ProductTypeId = entity.ProductTypeId,

            Name = entity.Name,

            Coefficient = entity.Coefficient,

        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Set<ProductType>().FirstOrDefaultAsync(x => x.ProductTypeId == id);
        if (entity is null) return NotFound();

        _db.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
