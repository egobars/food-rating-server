namespace food_rating_server.Models
{
    public class PublicComment
    {
        public int Id { get; set; }
        public int Mark { get; set; }
        public string Text { get; set; }
        public string Author { get; set; }
        public long Created { get; set; }
        public int Score { get; set; }
        public bool CanRate { get; set; }
    }
}
