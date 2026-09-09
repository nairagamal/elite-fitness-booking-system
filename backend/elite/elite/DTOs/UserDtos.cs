using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class UserRegisterDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress, MaxLength(100)]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Phone, MaxLength(20)]
        public string Phone { get; set; }
    }

    public class UserLoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public MembershipDto Membership { get; set; }
    }

    public class UserUpdateDto
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [Phone, MaxLength(20)]
        public string Phone { get; set; }
    }


    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
