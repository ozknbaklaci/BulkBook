using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}