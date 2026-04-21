using System.ComponentModel.DataAnnotations;

namespace APBD_Task5.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string BuildingCode { get; set; } = string.Empty;

        public int Floor { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than zero.")]
        public int Capacity { get; set; }

        public bool HasProjector { get; set; }

        public bool IsActive { get; set; }
    }
}