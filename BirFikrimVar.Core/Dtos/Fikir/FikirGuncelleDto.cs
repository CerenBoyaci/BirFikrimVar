using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BirFikrimVar.Core.Dtos.Fikir
{
    public class FikirGuncelleDto
    {
        public int Id { get; set; } //hangi fikirler güncellenecek bunu bilmem gerek

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        public string Baslik { get; set; }

        [Required(ErrorMessage = "Özet alanı zorunludur.")]
        public string Ozet { get; set; }

        [Required(ErrorMessage = "Kapsam alanı zorunludur.")]
        public string Kapsam { get; set; }

        [Required(ErrorMessage = "Yenilikçi Yanı zorunludur.")]
        public string YenilikciYan { get; set; }

        [Required(ErrorMessage = "Güçlü yönler alanı zorunludur.")]
        public string GucluYonler { get; set; }

        [Required(ErrorMessage = "Zayıf yönler alanı zorunludur.")]
        public string ZayifYonler { get; set; }

        [Required(ErrorMessage = "Fırsatlar alanı zorunludur.")]
        public string Firsatlar { get; set; }

        [Required(ErrorMessage = "Tehditler alanı zorunludur.")]
        public string Tehditler { get; set; }
    }
}
