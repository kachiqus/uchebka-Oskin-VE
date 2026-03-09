using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oskin_site.Data;
using oskin_site.Dtos;
using oskin_site.Models;
using oskin_site.Services;

namespace oskin_site.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly RawMaterialCalculatorService _rawMaterialCalculator;

    public ProductController(ApplicationDbContext db, RawMaterialCalculatorService rawMaterialCalculator)
    {
        _db = db;
        _rawMaterialCalculator = rawMaterialCalculator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Products
            .AsNoTracking()
            .Select(x => new ProductDto
            {
                ProductId = x.ProductId,
                Article = x.Article,
                Name = x.Name,
                ProductTypeId = x.ProductTypeId,
                ProductTypeName = x.ProductType.Name,
                MinPartnerPrice = x.MinPartnerPrice,
                MainMaterialTypeId = x.MainMaterialTypeId,
                MainMaterialTypeName = x.MainMaterialType.Name,
                ManufactureTimeHours = (int)Math.Ceiling(x.ProductWorkshops.Sum(pw => (decimal?)pw.ManufactureHours) ?? 0m)
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var item = await _db.Products
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .Select(x => new ProductDto
            {
                ProductId = x.ProductId,
                Article = x.Article,
                Name = x.Name,
                ProductTypeId = x.ProductTypeId,
                ProductTypeName = x.ProductType.Name,
                MinPartnerPrice = x.MinPartnerPrice,
                MainMaterialTypeId = x.MainMaterialTypeId,
                MainMaterialTypeName = x.MainMaterialType.Name,
                ManufactureTimeHours = (int)Math.Ceiling(x.ProductWorkshops.Sum(pw => (decimal?)pw.ManufactureHours) ?? 0m)
            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("{id:long}/workshops")]
    public async Task<IActionResult> GetWorkshops(long id)
    {
        var productExists = await _db.Products.AnyAsync(x => x.ProductId == id);
        if (!productExists)
            return NotFound("Продукция не найдена.");

        var items = await _db.ProductWorkshops
            .AsNoTracking()
            .Where(x => x.ProductId == id)
            .OrderBy(x => x.Workshop.Name)
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

    [HttpGet("{id:long}/manufacture-time")]
    public async Task<IActionResult> GetManufactureTime(long id)
    {
        var productExists = await _db.Products.AnyAsync(x => x.ProductId == id);
        if (!productExists)
            return NotFound("Продукция не найдена.");

        var total = await _db.ProductWorkshops
            .Where(x => x.ProductId == id)
            .SumAsync(x => (decimal?)x.ManufactureHours) ?? 0m;

        return Ok(new { productId = id, manufactureTimeHours = (int)Math.Ceiling(total) });
    }

    [HttpPost("calculate-material")]
    public async Task<IActionResult> CalculateMaterial([FromBody] MaterialCalculationRequestDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _rawMaterialCalculator.CalculateAsync(
            dto.ProductTypeId,
            dto.MaterialTypeId,
            dto.ProductCount,
            dto.Param1,
            dto.Param2);

        return Ok(new MaterialCalculationResultDto { Result = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var validationError = await ValidateReferences(dto.ProductTypeId, dto.MainMaterialTypeId, dto.Article, null);
        if (validationError is not null)
            return validationError;

        var entity = new Product
        {
            Article = dto.Article,
            Name = dto.Name.Trim(),
            ProductTypeId = dto.ProductTypeId,
            MinPartnerPrice = dto.MinPartnerPrice,
            MainMaterialTypeId = dto.MainMaterialTypeId,
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(await BuildProductDto(entity.ProductId));
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Patch(long id, [FromBody] JsonPatchDocument<ProductUpdateDto> patch)
    {
        if (patch is null)
            return BadRequest("Patch document is required.");

        var entity = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == id);
        if (entity is null)
            return NotFound();

        var dto = new ProductUpdateDto
        {
            Article = entity.Article,
            Name = entity.Name,
            ProductTypeId = entity.ProductTypeId,
            MinPartnerPrice = entity.MinPartnerPrice,
            MainMaterialTypeId = entity.MainMaterialTypeId,
        };

        patch.ApplyTo(dto, ModelState);
        TryValidateModel(dto);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var validationError = await ValidateReferences(dto.ProductTypeId, dto.MainMaterialTypeId, dto.Article, entity.ProductId);
        if (validationError is not null)
            return validationError;

        entity.Article = dto.Article;
        entity.Name = dto.Name.Trim();
        entity.ProductTypeId = dto.ProductTypeId;
        entity.MinPartnerPrice = dto.MinPartnerPrice;
        entity.MainMaterialTypeId = dto.MainMaterialTypeId;

        await _db.SaveChangesAsync();

        return Ok(await BuildProductDto(entity.ProductId));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == id);
        if (entity is null)
            return NotFound();

        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult?> ValidateReferences(int productTypeId, int materialTypeId, long article, long? currentProductId)
    {
        var productTypeExists = await _db.ProductTypes.AnyAsync(x => x.ProductTypeId == productTypeId);
        if (!productTypeExists)
            return BadRequest("Выбранный тип продукции не найден.");

        var materialTypeExists = await _db.MaterialTypes.AnyAsync(x => x.MaterialTypeId == materialTypeId);
        if (!materialTypeExists)
            return BadRequest("Выбранный основной материал не найден.");

        var duplicateArticle = await _db.Products.AnyAsync(x => x.Article == article && (!currentProductId.HasValue || x.ProductId != currentProductId.Value));
        if (duplicateArticle)
            return BadRequest("Продукция с таким артикулом уже существует.");

        return null;
    }

    private async Task<ProductDto> BuildProductDto(long productId)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => new ProductDto
            {
                ProductId = x.ProductId,
                Article = x.Article,
                Name = x.Name,
                ProductTypeId = x.ProductTypeId,
                ProductTypeName = x.ProductType.Name,
                MinPartnerPrice = x.MinPartnerPrice,
                MainMaterialTypeId = x.MainMaterialTypeId,
                MainMaterialTypeName = x.MainMaterialType.Name,
                ManufactureTimeHours = (int)Math.Ceiling(x.ProductWorkshops.Sum(pw => (decimal?)pw.ManufactureHours) ?? 0m)
            })
            .FirstAsync();
    }
}
