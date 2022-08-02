using food_rating_server.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace food_rating_server.Controllers
{
    public class AddController : Controller
    {
        private readonly FoodRatingDbContext _dBContext;

        public AddController()
        {
            _dBContext = new FoodRatingDbContext();
        }

        [HttpPost]
        async public Task<IActionResult> Product()
        {
            StreamReader stream = new StreamReader(Request.Body);
            string body = await stream.ReadToEndAsync();

            Product product = JsonConvert.DeserializeObject<Product>(body);

            if (product == null)
            {
                return StatusCode(406, "Product not found");
            }

            for (int i = 0; i < product.Image.Length; ++i)
            {
                if (product.Image[i] == ',')
                {
                    product.Image = product.Image.Substring(i + 1, product.Image.Length - i - 1);
                    break;
                }
            }

            var values = new Dictionary<string, string>
            {
                { "key", "a083c49cb011575ef83bb09e3c85c2ea" },
                { "image", product.Image },
                { "name", product.Name },
            };

            var content = new FormUrlEncodedContent(values);
            var response = await Constants.client.PostAsync("https://api.imgbb.com/1/upload", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var responceObject = JObject.Parse(responseString);

            product.Image = responceObject.SelectToken("$.data.url").Value<string>();

            _dBContext.Products.Add(product);
            _dBContext.SaveChanges();

            return Ok();
        }
        
        [HttpPost]
        async public Task<IActionResult> Comment()
        {
            StreamReader stream = new StreamReader(Request.Body);
            string body = await stream.ReadToEndAsync();

            Comment comment = JsonConvert.DeserializeObject<Comment>(body);

            if (comment == null)
            {
                return StatusCode(406, "Comment not found");
            }

            Product product = _dBContext.Products.Where(product => product.Id == comment.ProductId).FirstOrDefault();

            if (product == null)
            {
                return StatusCode(406, "Product not found");
            }

            User user = _dBContext.Users.Where(user => user.Login == comment.Author).FirstOrDefault();

            if (user == null)
            {
                return StatusCode(406, "User not found");
            }

            product.AllMark = product.AllMark + comment.Mark;
            product.AverageMark = product.AllMark / (product.CommentsCount + 1);
            ++product.CommentsCount;
            _dBContext.Products.Update(product);

            _dBContext.Comments.Add(comment);

            ++user.CommentsCount;
            _dBContext.Users.Update(user);

            _dBContext.SaveChanges();

            return Ok();
        }

        [HttpPost]
        async public Task<IActionResult> Change()
        {
            StreamReader stream = new StreamReader(Request.Body);
            string body = await stream.ReadToEndAsync();

            Change change = JsonConvert.DeserializeObject<Change>(body);

            if (change == null)
            {
                return StatusCode(406, "Change not found");
            }

            User user;
            ClaimsPrincipal principal = HttpContext.User;
            string login = "";
            if (null != principal)
            {
                if (!principal.Claims.Any())
                {
                    login = "";
                }
                else
                {
                    login = principal.Claims.First().Value.ToString();
                }
            }
            if (login != "")
            {
                user = _dBContext.Users.Where(user => user.Login == login).FirstOrDefault();
            }
            else
            {
                return StatusCode(406, "User not logged in");
            }

            change.UserId = user.Id;

            Comment comment = _dBContext.Comments.Where(comment => comment.Id == change.CommentId).FirstOrDefault();
            if (comment == null)
            {
                return StatusCode(406, "Comment not found");
            }

            User commentAuthor = _dBContext.Users.Where(user => user.Login == comment.Author).FirstOrDefault();
            if (commentAuthor == null)
            {
                return StatusCode(406, "Comment's author not found");
            }

            comment.Score += change.Value;
            _dBContext.Comments.Update(comment);

            commentAuthor.Karma += change.Value;
            _dBContext.Users.Update(commentAuthor);

            _dBContext.Changes.Add(change);

            _dBContext.SaveChanges();

            return Ok();
        }
    }
}
