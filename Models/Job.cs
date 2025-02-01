using System.ComponentModel.DataAnnotations;

namespace JobTracker.Models
{
    public class Job
    {
        public int Id { get; set; }

        [StringLength(100, MinimumLength = 2)]
        [Required]
        public required string Company { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Url)]
        public string? Link { get; set; }

        [Display(Name = "Application Date")]
        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }

        public string? Status { get; set; }

        public required string UserId {  get; set; }
    }
}
