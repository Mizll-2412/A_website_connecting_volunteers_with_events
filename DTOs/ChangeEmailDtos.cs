using System.ComponentModel.DataAnnotations;

namespace khoaluantotnghiep.DTOs
{
    public class ChangeEmailRequest
    {
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; } = string.Empty;
    }

    public class ConfirmChangeEmailRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}


