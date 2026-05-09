using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace BirFikrimVar.Core.Dtos.Auth
{
    public class AdminKullaniciOlusturDto
    {
        [Required]
        public string Ad { get; set; }
        [Required]
        public string Soyad { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Parola { get; set; }
        public string Rol { get; set; }
    }
}
