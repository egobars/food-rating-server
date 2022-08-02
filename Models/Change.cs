namespace food_rating_server.Models
{
    public class Change
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public short Value { get; set; }
    }
}
