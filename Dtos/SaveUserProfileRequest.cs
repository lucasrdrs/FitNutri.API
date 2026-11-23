using System.ComponentModel.DataAnnotations;

namespace br.com.FitNutri.Api.Dtos;

public sealed class SaveUserProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    [StringLength(100)]
    public string? Goal { get; set; }

    [StringLength(100)]
    public string? TrainingRoutine { get; set; }

    [MaxLength(20)]
    public List<string> DietaryRestrictions { get; set; } = [];

    [StringLength(300)]
    public string? OtherDietaryRestrictions { get; set; }
}
