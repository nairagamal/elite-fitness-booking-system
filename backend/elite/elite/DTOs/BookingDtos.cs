using System.ComponentModel.DataAnnotations;

namespace elite.DTOs
{
    public class CreateBookingDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public int ScheduleId { get; set; }
    }

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int ScheduleId { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }
        public decimal HoursConsumed { get; set; }
        public string ClassName { get; set; }
        public string TrainerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }

        // Add these properties for admin view
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; } // Add this
    }

    public class BookingCancellationDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}
