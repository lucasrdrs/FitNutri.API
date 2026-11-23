using System.Security.Claims;
using br.com.FitNutri.Api.Data;
using br.com.FitNutri.Api.Dtos;
using br.com.FitNutri.Api.Models;
using br.com.FitNutri.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace br.com.FitNutri.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProfileController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return Unauthorized(new { message = "Conta não encontrada." });
        }

        if (user.ProfileType == ProfileType.Usuario)
        {
            var profile = await _db.UserProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.UserId == userId);

            if (profile is null)
            {
                return NotFound(new
                {
                    message = "O perfil de usuário ainda não foi preenchido.",
                    profileType = "usuario"
                });
            }

            return Ok(ToResponse(profile));
        }

        if (user.ProfileType == ProfileType.Nutricionista)
        {
            var profile = await _db.NutritionistProfiles
                .AsNoTracking()
                .SingleOrDefaultAsync(profile => profile.UserId == userId);

            if (profile is null)
            {
                return NotFound(new
                {
                    message = "O perfil de nutricionista ainda não foi preenchido.",
                    profileType = "nutricionista"
                });
            }

            return Ok(ToResponse(profile));
        }

        return Conflict(new
        {
            message = "Esta conta ainda não escolheu um tipo de perfil."
        });
    }

    [HttpPut("user")]
    public async Task<IActionResult> SaveUserProfile(
        [FromBody] SaveUserProfileRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _db.Users
            .SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return Unauthorized(new { message = "Conta não encontrada." });
        }

        if (user.ProfileType != ProfileType.Usuario)
        {
            return Conflict(new
            {
                message = "Esta conta não está configurada como usuário."
            });
        }

        if (request.BirthDate.HasValue &&
            request.BirthDate.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return BadRequest(new
            {
                message = "A data de nascimento não pode ser futura."
            });
        }

        var profile = await _db.UserProfiles
            .SingleOrDefaultAsync(profile => profile.UserId == userId);

        if (profile is null)
        {
            profile = new UserProfile { UserId = userId };
            _db.UserProfiles.Add(profile);
        }

        profile.FullName = request.FullName.Trim();
        profile.BirthDate = request.BirthDate;
        profile.Goal = Clean(request.Goal);
        profile.TrainingRoutine = Clean(request.TrainingRoutine);
        profile.DietaryRestrictions = (request.DietaryRestrictions ?? [])
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        profile.OtherDietaryRestrictions =
            Clean(request.OtherDietaryRestrictions);

        await _db.SaveChangesAsync();

        return Ok(ToResponse(profile));
    }

    [HttpPut("nutritionist")]
    public async Task<IActionResult> SaveNutritionistProfile(
        [FromBody] SaveNutritionistProfileRequest request)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { message = "Token inválido." });
        }

        var user = await _db.Users
            .SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return Unauthorized(new { message = "Conta não encontrada." });
        }

        if (user.ProfileType != ProfileType.Nutricionista)
        {
            return Conflict(new
            {
                message = "Esta conta não está configurada como nutricionista."
            });
        }

        var profile = await _db.NutritionistProfiles
            .SingleOrDefaultAsync(profile => profile.UserId == userId);

        var crnRegion = request.CrnRegion.Trim();
        var crnNumber = request.CrnNumber.Trim();

        if (profile is null)
        {
            profile = new NutritionistProfile
            {
                UserId = userId,
                VerificationStatus = CrnVerificationStatus.Pendente
            };

            _db.NutritionistProfiles.Add(profile);
        }
        else
        {
            var crnChanged =
                !string.Equals(
                    profile.CrnRegion,
                    crnRegion,
                    StringComparison.OrdinalIgnoreCase)
                || !string.Equals(
                    profile.CrnNumber,
                    crnNumber,
                    StringComparison.Ordinal);

            if (crnChanged)
            {
                profile.VerificationStatus =
                    CrnVerificationStatus.Pendente;
            }
        }

        profile.FullName = request.FullName.Trim();
        profile.CrnRegion = crnRegion;
        profile.CrnNumber = crnNumber;
        profile.Specialty = request.Specialty.Trim();
        profile.OtherSpecialty = Clean(request.OtherSpecialty);
        profile.Presentation = request.Presentation.Trim();

        await _db.SaveChangesAsync();

        return Ok(ToResponse(profile));
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return int.TryParse(userIdClaim, out userId);
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static object ToResponse(UserProfile profile)
    {
        return new
        {
            tipo = "usuario",
            nome = profile.FullName,
            nascimento = profile.BirthDate?.ToString("yyyy-MM-dd"),
            objetivo = profile.Goal,
            rotina = profile.TrainingRoutine,
            restricoes = profile.DietaryRestrictions,
            outrasRestricoes = profile.OtherDietaryRestrictions
        };
    }

    private static object ToResponse(NutritionistProfile profile)
    {
        var status = profile.VerificationStatus switch
        {
            CrnVerificationStatus.Aprovado => "verificado",
            CrnVerificationStatus.Rejeitado => "rejeitado",
            _ => "pendente"
        };

        return new
        {
            tipo = "nutricionista",
            nome = profile.FullName,
            crnRegiao = profile.CrnRegion,
            crnNumero = profile.CrnNumber,
            especialidade = profile.Specialty,
            especialidadeOutra = profile.OtherSpecialty,
            apresentacao = profile.Presentation,
            statusVerificacao = status
        };
    }
}
