using System.ComponentModel.DataAnnotations;

namespace br.com.FitNutri.Api.Dtos;

public sealed class LoginUserRequest {
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
