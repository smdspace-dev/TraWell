using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class Video
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public string ThumbnailUrl { get; set; } = string.Empty;
        
        public DateTime PublishedAt { get; set; }
        
        [MaxLength(100)]
        public string ChannelTitle { get; set; } = string.Empty;
        
        public long ViewCount { get; set; } = 0;
        
        public int Duration { get; set; } = 0; // Duration in seconds
        
        [MaxLength(100)]
        public string PlaceName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
