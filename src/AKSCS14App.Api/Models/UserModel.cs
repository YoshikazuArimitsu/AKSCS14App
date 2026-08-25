using System.ComponentModel.DataAnnotations;

using CS14App.Api.Compliance;

namespace CS14App.Api.Models;

public class UserModel
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [PhoneNumberData]
    public string PhoneNumber { get; set; } = string.Empty;
}