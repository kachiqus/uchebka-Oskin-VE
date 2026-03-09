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

public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public UserController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.Set<User>()
            .AsNoTracking()
            .Select(x => new UserDto
            {

                UsersId = x.UsersId,

                Username = x.Username,

                PasswordHash = x.PasswordHash,

                Role = x.Role,

                CreatedAt = x.CreatedAt,

            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.Set<User>()
            .AsNoTracking()
            .Where(x => x.UsersId == id)
            .Select(x => new UserDto
            {

                UsersId = x.UsersId,

                Username = x.Username,

                PasswordHash = x.PasswordHash,

                Role = x.Role,

                CreatedAt = x.CreatedAt,

            })
            .FirstOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UserCreateDto dto)
    {
        var entity = new User
        {




            Username = dto.Username,



            PasswordHash = dto.PasswordHash,



            Role = dto.Role,



            CreatedAt = dto.CreatedAt,


        };

        _db.Set<User>().Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {

            UsersId = entity.UsersId,

            Username = entity.Username,

            PasswordHash = entity.PasswordHash,

            Role = entity.Role,

            CreatedAt = entity.CreatedAt,

        });
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<UserUpdateDto> patch)
    {
        if (patch is null) return BadRequest("Patch document is required.");

        var entity = await _db.Set<User>().FirstOrDefaultAsync(x => x.UsersId == id);
        if (entity is null) return NotFound();

        var dto = new UserUpdateDto
        {




            Username = entity.Username,



            PasswordHash = entity.PasswordHash,



            Role = entity.Role,



            CreatedAt = entity.CreatedAt,


        };

        patch.ApplyTo(dto, ModelState);
        if (!ModelState.IsValid) return ValidationProblem(ModelState);





        entity.Username = dto.Username;



        entity.PasswordHash = dto.PasswordHash;



        entity.Role = dto.Role;



        entity.CreatedAt = dto.CreatedAt;



        await _db.SaveChangesAsync();

        return Ok(new UserDto
        {

            UsersId = entity.UsersId,

            Username = entity.Username,

            PasswordHash = entity.PasswordHash,

            Role = entity.Role,

            CreatedAt = entity.CreatedAt,

        });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Set<User>().FirstOrDefaultAsync(x => x.UsersId == id);
        if (entity is null) return NotFound();

        _db.Remove(entity);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
