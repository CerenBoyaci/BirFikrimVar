using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Core.Dtos.Fikir
{
    public class KomisyonPuanEkleDto
    {
        [Range(1, 10, ErrorMessage = "Puan 1 ile 10 arasında olmalıdır.")]
        public int Puan { get; set; }
    }
}
