
using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Web.Models
{
    public class KategoriViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur.")]
        public string Ad { get; set; }

        public bool AktifMi { get; set; }
    }

    public class OverrideStatusViewModel
    {
        public int FikirId { get; set; }

        [Required]
        public int YeniDurum { get; set; }

        [Required(ErrorMessage = "Lütfen müdahale sebebini loglanmak üzere belirtin.")]
        public string Aciklama { get; set; }
    }
}
