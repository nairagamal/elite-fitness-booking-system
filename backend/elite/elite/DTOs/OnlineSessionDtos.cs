using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class OnlineSessionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Duration { get; set; } // Now in minutes
        public int TrainerId { get; set; }
        public string TrainerName { get; set; }
        public decimal Price { get; set; }
    }

    // DTOs/OnlineSessionCreateDto.cs
    public class OnlineSessionCreateDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(15, 300)] // Updated range for minutes (15 min to 5 hours)
        public decimal Duration { get; set; } // Now in minutes

        [Required]
        public int TrainerId { get; set; }

        public decimal Price { get; set; }
    }

    public class OnlineSessionScheduleDto
    {
        public int Id { get; set; }
        public int OnlineSessionId { get; set; }
        public string SessionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AvailableSlots { get; set; }
        public int MaxSlots { get; set; } = 20;
    }

    public class OnlineSessionScheduleCreateDto
    {
        [Required]
        public int OnlineSessionId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Range(1, 20)]
        public int AvailableSlots { get; set; } = 20;
    }
}
