namespace oskin_site.Dtos;

public sealed class UserDto
{

    public int UsersId { get; set; }

    public string Username { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; }

    public DateTime CreatedAt { get; set; }

}
