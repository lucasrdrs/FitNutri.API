using System.ComponentModel.DataAnnotations;
using br.com.FitNutri.Api.Dtos;
using br.com.FitNutri.Api.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace br.com.FitNutri.Api.Dtos;

public class SetProfileTypeRequest
{
    [Required]
    public string ProfileType { get; set; } = string.Empty;
}
