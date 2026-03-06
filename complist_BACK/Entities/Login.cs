using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace complist_BACK.Entities
{
    public class Login
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string LoginName { get; set; }

        [Required]
        [MaxLength(10)]
        public string Password { get; set; }
    }
}

