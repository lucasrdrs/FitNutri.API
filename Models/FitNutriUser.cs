using br.com.FitNutri.Models;
using Microsoft.AspNetCore.Identity;

namespace br.com.FitNutri.Api.Models;

public class FitNutriUser : IdentityUser<int>
{
    public ProfileType? ProfileType { get; set; }
}