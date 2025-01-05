using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace project.Data
{
    public class Post
    {
        public int id { get; set; }
        
        [Required]
        public string username { get; set; }

        public bool isPublic { get; set; } = false;

        public bool isVerified { get; set; } = false;

        [Required(ErrorMessage = "Title is required.")]
        public string title { get; set; }

        public string text { get; set; } = "";

        public int hearts { get; set; } = 0;

        [Required]
        public DateTime createdAt { get; set; }
    }
}
