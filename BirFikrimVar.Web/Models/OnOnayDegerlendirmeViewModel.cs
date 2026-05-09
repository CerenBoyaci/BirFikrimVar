using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class OnOnayDegerlendirmeViewModel
    {
        public int FikirId { get; set; }
        public FikirDetayViewModel? Fikir { get; set; }

        public List<KategoriPuanViewModel> KategoriPuanlari { get; set; } = new List<KategoriPuanViewModel>();
    }

    public class KategoriPuanViewModel
    {
        public int KategoriId { get; set; }
        public string? KategoriAdi { get; set; }

        [Required(ErrorMessage = "Puan alanı zorunludur.")]
        [Range(1, 10, ErrorMessage = "Lütfen 1 ile 10 arasında bir puan giriniz.")]
        public int Puan { get; set; }
    }
}
