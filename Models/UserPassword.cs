using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace loginWeb1.Models
{
    public class UserPassword
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}