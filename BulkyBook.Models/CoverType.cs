using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class CoverType : BaseEntity
    {
        [Display(Name = "Cover Type")]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}
