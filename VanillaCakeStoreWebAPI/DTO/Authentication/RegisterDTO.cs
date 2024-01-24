using System.ComponentModel.DataAnnotations;

namespace VanillaCakeStoreWebAPI.DTO.Authentication
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is required!")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Confirmation Password is required!")]
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string RePassword { get; set; } = null!;
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? ContactTitle { get; set; }
        public string? Address { get; set; }

    }
}
