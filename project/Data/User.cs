using System.ComponentModel.DataAnnotations;

namespace project.Data
{
    public class User
    {
        public int id { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string username { get; set; }

        [Required]
        public string password { get; set; }
    }
}
