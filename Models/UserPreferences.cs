using System.ComponentModel.DataAnnotations;

namespace TraWell.Models
{
    public class UserPreferences
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public List<string> PreferredCategories { get; set; } = new List<string>();
        
        public List<string> Interests { get; set; } = new List<string>();
        
        public List<string> Activities { get; set; } = new List<string>(); // For backward compatibility
        
        public string Season { get; set; } = string.Empty; // For backward compatibility
        
        public int Duration { get; set; } // For backward compatibility
        
        [Range(1, 5)]
        public int BudgetRange { get; set; } = 3; // 1=Budget, 2=Economy, 3=Standard, 4=Premium, 5=Luxury
        
        [MaxLength(20)]
        public string PreferredSeason { get; set; } = string.Empty;
        
        [Range(1, 30)]
        public int PreferredDuration { get; set; } = 7; // Duration in days
        
        public bool LikesAdventure { get; set; } = false;
        
        public bool LikesRelaxation { get; set; } = false;
        
        public bool LikesCulture { get; set; } = false;
        
        public bool LikesNature { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
    }
}
