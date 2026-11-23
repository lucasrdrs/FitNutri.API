using System.ComponentModel.DataAnnotations;

namespace br.com.FitNutri.Api.Models;

public class NutritionistProfile
{
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string CrnRegion { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string CrnNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Specialty { get; set; } = string.Empty;

    [StringLength(100)]
    public string? OtherSpecialty { get; set; }

    [Required]
    [StringLength(2000)]
    public string Presentation { get; set; } = string.Empty;

    public CrnVerificationStatus VerificationStatus { get; set; }
        = CrnVerificationStatus.Pendente;

    public FitNutriUser User { get; set; } = null!;
}
