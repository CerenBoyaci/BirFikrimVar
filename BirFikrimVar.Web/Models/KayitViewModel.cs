using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class KayitViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola alanı zorunludur.")]
        [MinLength(8, ErrorMessage = "Parola en az 8 karakter olmalıdır.")]
        public string Parola { get; set; }

        [Compare("Parola", ErrorMessage = "Parolalar eşleşmiyor.")]
        public string ParolaTekrar { get; set; }
    }

    public class GirisViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta formatı giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Parola zorunludur.")]
        public string Parola { get; set; }
    }
}
