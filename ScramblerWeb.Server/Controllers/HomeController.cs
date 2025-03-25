using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        [HttpGet("generateKey")]
        public JsonResult GenerateKey(int length)
        {
            return Json(_keyGenerator.Generate(length));
        }
        private string BytesToHex(byte[] bytes)
        {
            return string.Concat(bytes.Select(b => b.ToString("x2")));
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
        [HttpPost("scrambleFile")]
        public async Task<IActionResult> ScrambleFile(IFormFile file, [FromForm] string key, [FromForm] string algorithms)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не вибрано або він пустий.");
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                return BadRequest("Ключ не може бути пустим.");
            }

            if (string.IsNullOrWhiteSpace(algorithms))
            {
                return BadRequest("Алгоритми не вибрані.");
            }

            List<ScramblerType> selectedAlgorithms = JsonSerializer.Deserialize<List<ScramblerType>>(algorithms);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();

            var keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] scrambledData = fileBytes;

            foreach (var algorithm in selectedAlgorithms)
            {
                var scrambler = _scramblerResolver(algorithm);
                scrambledData = scrambler.Scramble(scrambledData, keyBytes);
            }

            return File(scrambledData, "application/octet-stream", "scrambled_" + file.FileName);
        }
        [HttpPost("unscrambleFile")]
        public async Task<IActionResult> UnscrambleFile(IFormFile file, [FromForm] string key, [FromForm] string algorithms)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Файл не вибрано або він пустий.");

                if (!file.FileName.StartsWith("scrambled_"))
                    return BadRequest("Файл не є заскрембльованим (очікується префікс 'scrambled_').");

                if (string.IsNullOrWhiteSpace(key))
                    return BadRequest("Ключ не може бути пустим.");

                if (string.IsNullOrWhiteSpace(algorithms))
                    return BadRequest("Алгоритми не вибрані.");

                List<ScramblerType> selectedAlgorithms = JsonSerializer.Deserialize<List<ScramblerType>>(algorithms);


                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();

                var keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] unscrambledData = fileBytes;

                selectedAlgorithms.Reverse();

                foreach (var algorithm in selectedAlgorithms)
                {
                    var scrambler = _scramblerResolver(algorithm);
                    unscrambledData = scrambler.Descramble(unscrambledData, keyBytes);
                }
                
                string newFileName = "unscrambled_" + file.FileName.Substring(10);

                return File(unscrambledData, "application/octet-stream", newFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутрішня помилка сервера: {ex.Message}");
            }
        }
    }
}
