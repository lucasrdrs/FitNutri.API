using br.com.FitNutri.Api.Dtos;
using br.com.FitNutri.Api.Models;
using br.com.FitNutri.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;



namespace br.com.FitNutri.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase {
    private readonly UserManager<FitNutriUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<FitNutriUser> userManager,
        IConfiguration configuration) {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request) {
        var user = new FitNutriUser {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
                ? null
                : request.PhoneNumber.Trim()
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded) {
            return BadRequest(new {
                message = "Não foi possível criar a conta.",
                errors = result.Errors.Select(error => error.Description)
            });
        }

        var token = GerarToken(user);

        return StatusCode(201, new
        {
            message = "Conta criada com sucesso.",
            accessToken = token.AccessToken,
            tokenType = token.TokenType,
            expiresAtUtc = token.ExpiresAtUtc
        });
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null ||
            !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new
            {
                message = "E-mail ou senha inválidos."
            });
        }

        return Ok(GerarToken(user));
    }


    private AuthTokenResponse GerarToken(FitNutriUser user)
    {
        var keyBase64 = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não foi configurada.");

        var keyBytes = Convert.FromBase64String(keyBase64);
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var now = DateTime.UtcNow;
        var expirationMinutes =
            _configuration.GetValue<int>("Jwt:ExpirationMinutes", 30);
        var expiresAtUtc = now.AddMinutes(expirationMinutes);

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthTokenResponse(
            accessToken,
            "Bearer",
            expiresAtUtc);
    }

    private sealed record AuthTokenResponse(
        string AccessToken,
        string TokenType,
        DateTime ExpiresAtUtc);

    [Authorize]
    [HttpPost("profile-type")]
    public async Task<IActionResult> SetProfileType(
    [FromBody] SetProfileTypeRequest request)
    {
        if (!Enum.TryParse<ProfileType>(
                request.ProfileType.Trim(),
                ignoreCase: true,
                out var profileType) ||
            !Enum.IsDefined(typeof(ProfileType), profileType))
        {
            return BadRequest(new
            {
                message = "Tipo de perfil inválido. Use usuario ou nutricionista."
            });
        }

        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return NotFound(new { message = "Conta não encontrada." });
        }

        if (user.ProfileType.HasValue)
        {
            if (user.ProfileType.Value == profileType)
            {
                return Ok(new { profileType = profileType.ToString() });
            }

            return Conflict(new
            {
                message = "Esta conta já possui um tipo de perfil definido."
            });
        }

        user.ProfileType = profileType;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Não foi possível salvar o tipo de perfil.",
                errors = result.Errors.Select(error => error.Description)
            });
        }

        return Ok(new
        {
            message = "Tipo de perfil salvo.",
            profileType = profileType.ToString()
        });
    }
    [Authorize]
    [HttpGet("profile-type")]
    public async Task<IActionResult> GetProfileType()
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return NotFound(new { message = "Conta não encontrada." });
        }

        return Ok(new
        {
            profileType = user.ProfileType?.ToString()
        });
    }


}
