using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class TrainerUpdateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Specialization { get; set; }

        [Required, Range(0, 50)]
        public int ExperienceYears { get; set; }

        public string Certifications { get; set; }

        [Required]
        public string Bio { get; set; }

        public IFormFile? ImageFile { get; set; } // Optional for updates
    }
}
