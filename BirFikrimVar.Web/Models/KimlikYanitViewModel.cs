namespace BirFikrimVar.Web.Models
{
    public class KimlikYanitViewModel
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpireDate { get; set; }
        public string Email { get; set; }
        public string Roller { get; set; }
    }

    public class ApiMesajViewModel
    {
        public string Mesaj { get; set; }
    }
}
