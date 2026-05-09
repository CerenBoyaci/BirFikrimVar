using System;
using System.Collections.Generic;
using System.Text;

namespace BirFikrimVar.Core.Dtos.Auth
{
    public class DogrulamaCevapDto
    {
        public string Token { get; set; }
        public DateTime TokenExpireDate { get; set; }
        public string Email { get; set; }
        public string Roller { get; set; }
    }
}
