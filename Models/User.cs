using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace loginWeb1.Models
{
    public class User
    {
        [Key] // Anahtar alan
        public int Id { get; set; }

        [Required] // Zorunlu alan
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [Index(IsUnique = true )] // Eşsiz indeks
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        [Compare("PasswordHash", ErrorMessage = "Şifre uyuşmuyor.")]
        public string confirmPassword { get; set; }
        [ReadOnly(true)]
        public bool IsVerified { get; set; } = false; // Varsayılan değer
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public string VerificationToken { get; set; }
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public DateTime PasswordLastChanged { get; set; } // Şifre değişiklik tarihi
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public DateTime? VerificationTokenExpiry { get; set; }
        public virtual ICollection<UserPassword> PreviousPasswords { get; set; } // Önceki şifreler
    }
}