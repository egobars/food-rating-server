using food_rating_server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace food_rating_server.Controllers
{
    public class GetController : Controller
    {
        private readonly FoodRatingDbContext _dBContext;

        public GetController()
        {
            _dBContext = new FoodRatingDbContext();
        }

        [HttpGet]
        [Route("api/get/products/{search?}")]
        async public Task<JsonResult> Products(string search)
        {
            List<Product> productsList;
            if (search == null)
            {
                productsList = _dBContext.Products.ToList();
                return Json(new { products = productsList });
            }
            search = @"\w*" + search + @"\w*";
            Regex regex = new Regex(search, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var allProductNamesList = await _dBContext.Products.Select(product => product.Name).ToListAsync();
            var productsNameList = allProductNamesList.Where(name => regex.Matches(name).Count != 0);

            productsList = new List<Product>();

            foreach (string productName in productsNameList)
            {
                productsList.Add(_dBContext.Products.Where(product => product.Name == productName).FirstOrDefault());
            }

            return Json(new
            {
                products = productsList
            });
        }

        [HttpGet]
        public JsonResult Product(int id)
        {
            Product product = _dBContext.Products.Where(product => product.Id == id).FirstOrDefault();

            return Json(new {
                product = product
            });
        }

        [HttpGet]
        public JsonResult Comments(int id)
        {
            List<Comment> comments = new List<Comment>();
            if (_dBContext.Comments.Where(comment => comment.ProductId == id).Count() != 0)
            {
                comments = _dBContext.Comments.Where(comment => comment.ProductId == id).ToList();
            }
            List<PublicComment> publicComments = new List<PublicComment>();

            User? user;
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
                user = null;
            }

            foreach (Comment comment in comments)
            {
                PublicComment publicComment = new PublicComment
                {
                    Id = comment.Id,
                    Mark = comment.Mark,
                    Text = comment.Text,
                    Author = comment.Author,
                    Created = comment.Created,
                    Score = comment.Score
                };

                if (user == null)
                {
                    publicComment.CanRate = false;
                } else
                {
                    if (_dBContext.Changes.Where(change =>
                        (change.CommentId == comment.Id) && (change.UserId == user.Id)
                    ).FirstOrDefault() == null)
                    {
                        publicComment.CanRate = true;
                    } else
                    {
                        publicComment.CanRate = false;
                    }
                }

                publicComments.Add(publicComment);
            }
            return Json(new {
                comments = publicComments
            });
        }

        [HttpGet]
        public JsonResult Users()
        {
            List<User> usersList = _dBContext.Users.ToList();
            List<PublicUser> publicUsersList = new List<PublicUser>();
            foreach (User user in usersList)
            {
                publicUsersList.Add(new PublicUser{
                    Login = user.Login,
                    CommentsCount = user.CommentsCount,
                    Karma = user.Karma
                });
            }
            return Json(new { users = publicUsersList });
        }
    }
}
