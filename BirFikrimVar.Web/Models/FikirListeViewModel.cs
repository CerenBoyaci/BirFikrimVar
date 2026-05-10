using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
 
    public class FikirListeViewModel
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Durum { get; set; }
        public string BasvuruSahibiAdSoyad { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
    }

  
    public class FikirOlusturViewModel
    {
        [Required(ErrorMessage = "Fikir başlığı zorunludur.")]
        public string Baslik { get; set; }

        [Required(ErrorMessage = "Özet alanı zorunludur.")]
        public string Ozet { get; set; }

        [Required(ErrorMessage = "Kapsam alanı zorunludur.")]
        public string Kapsam { get; set; }

        [Required(ErrorMessage = "Yenilikçi yan alanı zorunludur.")]
        public string YenilikciYan { get; set; }

     
        [Required(ErrorMessage = "Güçlü yönler (Strengths) zorunludur.")]
        public string GucluYonler { get; set; }

        [Required(ErrorMessage = "Zayıf yönler (Weaknesses) zorunludur.")]
        public string ZayifYonler { get; set; }

        [Required(ErrorMessage = "Fırsatlar (Opportunities) zorunludur.")]
        public string Firsatlar { get; set; }

        [Required(ErrorMessage = "Tehditler (Threats) zorunludur.")]
        public string Tehditler { get; set; }
    }


    public class FikirDetayViewModel : FikirOlusturViewModel
    {
        public int Id { get; set; }
        public string Durum { get; set; }
        public string BasvuruSahibiAdSoyad { get; set; }
        public DateTime OlusturmaTarihi { get; set; }
        public List<FikirDosyaViewModel> Dosyalar { get; set; } = new List<FikirDosyaViewModel>();
        public List<OnOnayPuanDetayViewModel> MevcutOnOnayPuanlari { get; set; } = new();
    }

    public class FikirDosyaViewModel
    {
        public int Id { get; set; }
        public string DosyaAdi { get; set; }
        public string Uzanti { get; set; }
    }
    public class OnOnayPuanDetayViewModel
    {
        public int KategoriId { get; set; }
        public double Puan { get; set; }
    }
}

