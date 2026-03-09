using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oskin_site.Data;
using oskin_site.Dtos;
using oskin_site.Models;

namespace oskin_site.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductWorkshopController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProductWorkshopController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.ProductWorkshops
            .AsNoTracking()
            .OrderBy(x => x.ProductId)
            .ThenBy(x => x.Workshop.Name)
            .Select(x => new ProductWorkshopDto
            {
                ProductId = x.ProductId,
                WorkshopId = x.WorkshopId,
                WorkshopName = x.Workshop.Name,
                PeopleCount = x.Workshop.PeopleCount,
                ManufactureHours = x.ManufactureHours
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{productId:long}/{workshopId:int}")]
    public async Task<IActionResult> GetById(long productId, int workshopId)
    {
        var item = await _db.ProductWorkshops
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.WorkshopId == workshopId)
            .Select(x => new ProductWorkshopDto
            {
                ProductId = x.ProductId,
                WorkshopId = x.WorkshopId,
                WorkshopName = x.Workshop.Name,
                PeopleCount = x.Workshop.PeopleCount,
                ManufactureHours = x.ManufactureHours
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductWorkshopCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var validationError = await ValidateReferences(dto.ProductId, dto.WorkshopId, null);
        if (validationError is not null)
            return validationError;

        var entity = new ProductWorkshop
        {
            ProductId = dto.ProductId,
            WorkshopId = dto.WorkshopId,
            ManufactureHours = dto.ManufactureHours,
        };

        _db.ProductWorkshops.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(await BuildDto(dto.ProductId, dto.WorkshopId));
    }

    [HttpPatch("{productId:long}/{workshopId:int}")]
    public async Task<IActionResult> Patch(long productId, int workshopId, [FromBody] JsonPatchDocument<ProductWorkshopUpdateDto> patch)
    {
        if (patch is null)
            return BadRequest("Patch document is required.");

        var entity = await _db.ProductWorkshops.FirstOrDefaultAsync(x => x.ProductId == productId && x.WorkshopId == workshopId);
        if (entity is null)
            return NotFound();

        var dto = new ProductWorkshopUpdateDto
        {
            ManufactureHours = entity.ManufactureHours,
        };

        patch.ApplyTo(dto, ModelState);
        TryValidateModel(dto);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        entity.ManufactureHours = dto.ManufactureHours;
        await _db.SaveChangesAsync();

        return Ok(await BuildDto(productId, workshopId));
    }

    [HttpDelete("{productId:long}/{workshopId:int}")]
    public async Task<IActionResult> Delete(long productId, int workshopId)
    {
        var entity = await _db.ProductWorkshops.FirstOrDefaultAsync(x => x.ProductId == productId && x.WorkshopId == workshopId);
        if (entity is null)
            return NotFound();

        _db.ProductWorkshops.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateReferences(long productId, int workshopId, object? _)
    {
        var productExists = await _db.Products.AnyAsync(x => x.ProductId == productId);
        if (!productExists)
            return BadRequest("Выбранная продукция не найдена.");

        var workshopExists = await _db.Workshops.AnyAsync(x => x.WorkshopId == workshopId);
        if (!workshopExists)
            return BadRequest("Выбранный цех не найден.");

        var duplicate = await _db.ProductWorkshops.AnyAsync(x => x.ProductId == productId && x.WorkshopId == workshopId);
        if (duplicate)
            return BadRequest("Для этой продукции связь с выбранным цехом уже существует.");

        return null;
    }

    private async Task<ProductWorkshopDto> BuildDto(long productId, int workshopId)
    {
        return await _db.ProductWorkshops
            .AsNoTracking()
            .Where(x => x.ProductId == productId && x.WorkshopId == workshopId)
            .Select(x => new ProductWorkshopDto
            {
                ProductId = x.ProductId,
                WorkshopId = x.WorkshopId,
                WorkshopName = x.Workshop.Name,
                PeopleCount = x.Workshop.PeopleCount,
                ManufactureHours = x.ManufactureHours
            })
            .FirstAsync();
    }
}
