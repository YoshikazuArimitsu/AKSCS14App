using System.ComponentModel.DataAnnotations;

namespace CS14App.Api.Models;

public class SendMessageRequest
{
    [Required]
    public string Text { get; set; } = string.Empty;
}
