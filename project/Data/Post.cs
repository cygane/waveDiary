namespace project.Data
{
    public class Post
    {
        public int id { get; set; }
        public string userId { get; set; }
        public bool isPublic { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string photo { get; set; }
        public int hearts { get; set; }
        public DateTime createdAt { get; set; }
    }
}
