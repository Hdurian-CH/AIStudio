using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Authentication
{
    public class LoginModel
    {
        [StringLength(20,MinimumLength = 5,ErrorMessage = "MaxLength:20,MiniLength:5")]
        public string UserName { get; set; }

        [StringLength(20,MinimumLength = 8,ErrorMessage = "Max:20 Min:8")]
        public string Password { get; set; }
    }
}
