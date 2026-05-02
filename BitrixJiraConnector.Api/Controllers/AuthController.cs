using BitrixJiraConnector.Api.Helpers;
using BitrixJiraConnector.Api.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BitrixJiraConnector.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db) : ControllerBase
{
    // POST /api/auth/login  (public — no X-Api-Key required)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username và password không được để trống." });

        var user = await db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Sai username hoặc password." });

        return Ok(new { success = true, role = "admin", username = user.Username });
    }

    // GET /api/auth/users  (requires X-Api-Key)
    [HttpGet("users")]
    public async Task<IActionResult> ListUsers()
    {
        var users = await db.AdminUsers
            .OrderBy(u => u.Id)
            .Select(u => new { u.Id, u.Username })
            .ToListAsync();
        return Ok(users);
    }

    // POST /api/auth/users  (requires X-Api-Key)
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username và password không được để trống." });

        if (await db.AdminUsers.AnyAsync(u => u.Username == request.Username))
            return Conflict(new { message = $"Username '{request.Username}' đã tồn tại." });

        var user = new AdminUser
        {
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(request.Password),
        };
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username });
    }

    // PUT /api/auth/users/{id}  (requires X-Api-Key)
    [HttpPut("users/{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound(new { message = "Không tìm thấy user." });

        if (!string.IsNullOrWhiteSpace(request.NewUsername))
        {
            bool taken = await db.AdminUsers.AnyAsync(u => u.Username == request.NewUsername && u.Id != id);
            if (taken) return Conflict(new { message = $"Username '{request.NewUsername}' đã tồn tại." });
            user.Username = request.NewUsername.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            user.PasswordHash = PasswordHasher.Hash(request.NewPassword);

        await db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username });
    }

    // DELETE /api/auth/users/{id}  (requires X-Api-Key)
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await db.AdminUsers.FindAsync(id);
        if (user is null) return NotFound(new { message = "Không tìm thấy user." });

        if (await db.AdminUsers.CountAsync() <= 1)
            return BadRequest(new { message = "Không thể xóa tài khoản admin cuối cùng." });

        db.AdminUsers.Remove(user);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record LoginRequest(string Username, string Password);
public record CreateUserRequest(string Username, string Password);
public record UpdateUserRequest(string? NewUsername, string? NewPassword);

