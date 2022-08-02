namespace food_rating_server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int CommentsCount { get; set; }
        public int Karma { get; set; }
    }
}
