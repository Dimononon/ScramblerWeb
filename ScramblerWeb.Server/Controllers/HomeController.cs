using Microsoft.AspNetCore.Mvc;
using ScramblerWeb.Server.Models;
using Services.Keys;
using Services.Scramblers;
using System.Text;

namespace ScramblerWeb.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly Func<ScramblerType, IScrambler> _scramblerResolver;
        private readonly IKeyGenerator _keyGenerator;
        public HomeController(Func<ScramblerType, IScrambler> scramblerResolver, IKeyGenerator keyGenerator) 
        {
            _scramblerResolver = scramblerResolver;
            _keyGenerator = keyGenerator;
        }
        [HttpPost("scramble")]
        public JsonResult Scramble(ByteForm form)
        {

            var keyBytes = Encoding.UTF8.GetBytes(form.Key);
            byte[] result = form.Data.ToArray();
            foreach (var algorithm in form.Algorithms)
            {
                var scrambler = _scramblerResolver(algorithm);
                result = scrambler.Scramble(result, keyBytes);
            }
           
            return Json(BytesToHex(result));
        }
        [HttpGet("generateKey")]
        public JsonResult GenerateKey(int length)
        {
            return Json(_keyGenerator.Generate(length));
        }
        private string BytesToHex(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }
        [HttpPost("unscramble")]
        public JsonResult UnscrambleByte(ByteForm form)
        {
            var keyBytes = Encoding.UTF8.GetBytes(form.Key);
            byte[] result = form.Data.ToArray();

            form.Algorithms.Reverse();
            foreach (var algorithm in form.Algorithms)
            {
                var scrambler = _scramblerResolver(algorithm);
                result = scrambler.Descramble(result, keyBytes);
            }

            return Json(Convert.ToBase64String(result));
        }
    }
}
