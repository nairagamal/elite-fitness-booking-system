using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    // DTOs/ClassDto.cs
    public class ClassDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Duration { get; set; } // Now in minutes
        public int TrainerId { get; set; }
        public string TrainerName { get; set; }
        public int MaxCapacity { get; set; }
        public decimal Price { get; set; }
    }

    // DTOs/ClassCreateDto.cs
    public class ClassCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(15, 300)] // Updated range for minutes (15 min to 5 hours)
        public decimal Duration { get; set; } // Now in minutes

        [Required]
        public int TrainerId { get; set; }

        [Required, Range(1, 50)]
        public int MaxCapacity { get; set; }

        public decimal Price { get; set; }
    }

    public class ClassScheduleDto
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public int AvailableSlots { get; set; }
        public int MaxCapacity { get; set; }
    }

    public class ClassScheduleCreateDto
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required, MaxLength(200)]
        public string Location { get; set; }
    }
}
