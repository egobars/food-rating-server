using food_rating_server.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace food_rating_server.Controllers
{
    public class AccountController : Controller
    {
        private readonly FoodRatingDbContext _dBContext;

        public AccountController()
        {
            _dBContext = new FoodRatingDbContext();
        }

        [HttpPost]
        public async Task<IActionResult> Register()
        {
            StreamReader stream = new StreamReader(Request.Body);
            string body = await stream.ReadToEndAsync();

            User user = JsonConvert.DeserializeObject<User>(body);

            if (user == null)
            {
                return StatusCode(406, "Empty user");
            }

            string nowLogin = user.Login;

            User dBUser = _dBContext.Users.Where(user => user.Login == nowLogin).FirstOrDefault();

            if (dBUser != null && dBUser.Login != null)
            {
                return StatusCode(406, "This login already exists");
            }

            user.CommentsCount = 0;
            user.Karma = 0;
            _dBContext.Users.Add(user);
            _dBContext.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login),
            };

            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { });

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> Login()
        {
            StreamReader stream = new StreamReader(Request.Body);
            string body = await stream.ReadToEndAsync();

            User user = JsonConvert.DeserializeObject<User>(body);

            if (user == null)
            {
                return StatusCode(406, "Empty user");
            }

            string nowLogin = user.Login;

            User dBUser = _dBContext.Users.Where(user => user.Login == nowLogin).FirstOrDefault();

            if (dBUser == null)
            {
                return StatusCode(406, "User not found");
            }

            if (dBUser.Password != user.Password)
            {
                return StatusCode(406, "Wrong password");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, dBUser.Login),
            };

            var claimsIdentity = new ClaimsIdentity(
            claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { });

            return Ok(dBUser);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
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
                User dBUser = _dBContext.Users.Where(user => user.Login == login).FirstOrDefault();
                return Ok(dBUser);
            } else
            {
                return StatusCode(406, "User not found");
            }
        }
    }
}
