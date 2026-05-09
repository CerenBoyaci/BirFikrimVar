using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class KomisyonDegerlendirmeViewModel
    {
        public int FikirId { get; set; }
        public FikirDetayViewModel? Fikir { get; set; }

        [Required(ErrorMessage = "Puan alanı zorunludur.")]
        [Range(1, 10, ErrorMessage = "Lütfen 1 ile 10 arasında bir puan giriniz.")]
        public int Puan { get; set; }
    }
}
