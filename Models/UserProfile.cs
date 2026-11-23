using br.com.FitNutri.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace br.com.FitNutri.Models
{
    public class UserProfile
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateOnly? BirthDate { get; set; }

        [StringLength(100)]
        public string? Goal { get; set; }

        [StringLength(100)]
        public string? TrainingRoutine { get; set; }

        public List<string> DietaryRestrictions { get; set; } = [];

        [StringLength(300)]
        public string? OtherDietaryRestrictions { get; set; }

        public FitNutriUser User { get; set; } = null!;
    }
}