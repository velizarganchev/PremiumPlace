using System.ComponentModel.DataAnnotations;

namespace PremiumPlace_API.Models
{
    public class Amenitie
    {
        public Amenitie()
        {
            Places = new HashSet<Place>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Place> Places { get; set; }
    }
}
