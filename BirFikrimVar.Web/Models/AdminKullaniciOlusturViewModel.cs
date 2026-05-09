using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class AdminKullaniciOlusturViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola zorunludur.")]
        [MinLength(6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
        public string Parola { get; set; }

        public string Rol { get; set; }
    }
}