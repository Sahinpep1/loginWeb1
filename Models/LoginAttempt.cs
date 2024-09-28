using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace loginWeb1.Models
{
    public class LoginAttempt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public DateTime AttemptedAt { get; set; }

        [Required]
        public bool IsSuccessful { get; set; }

        public string FailureReason { get; set; } // Eğer giriş başarısızsa nedeni burada tutabiliriz
    }
}