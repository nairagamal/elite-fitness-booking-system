using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class TrainerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public int ExperienceYears { get; set; }
        public string Certifications { get; set; }
        public string Bio { get; set; }
        public string ImageUrl { get; set; }
    }

    public class TrainerCreateDto
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
        public IFormFile ImageFile { get; set; } // For file upload
    }
}
