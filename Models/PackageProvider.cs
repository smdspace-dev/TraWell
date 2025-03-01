using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class PackageProvider
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string ContactPersonName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string ContactEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ContactPhone { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LicenseNumber { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public decimal CommissionRate { get; set; } = 0.10m; // 10% default commission

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<TourPackage> TourPackages { get; set; } = new List<TourPackage>();
    }
}
