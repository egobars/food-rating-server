namespace food_rating_server.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public double AverageMark { get; set; }
        public double AllMark { get; set; }
        public int CommentsCount { get; set; }
    }
}
