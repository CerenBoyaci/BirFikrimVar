using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class ProfilGuncelleViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta Adresi")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? MevcutParola { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        [MinLength(8, ErrorMessage = "Yeni şifre en az 8 karakter olmalıdır.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.")]
        public string? YeniParola { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("YeniParola", ErrorMessage = "Girdiğiniz şifreler birbiriyle eşleşmiyor.")]
        public string? YeniParolaTekrar { get; set; }
    }
}
