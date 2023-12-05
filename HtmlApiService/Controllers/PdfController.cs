using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using System.Net.Mime;

namespace HtmlApiService.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly string _tempDirectory = "PdfTemp";

        public PdfController()
        {
            if (!Directory.Exists(_tempDirectory))
            {
                Directory.CreateDirectory(_tempDirectory);
            }
        }
        [RequestSizeLimit(long.MaxValue)]
        [HttpPost("ConvertHtmlToPdf")]
        public IActionResult ConvertHtmlToPdf(IFormFile File)
        {
            try
            {
                if (File == null)
                {
                    return BadRequest("No file uploaded.");
                }

                var conversionId = Guid.NewGuid().ToString();
                BackgroundJob.Enqueue(() => ConvertAndSavePdf(File.FileName, GetFileBytes(File), conversionId));

                return Ok(new { ConversionId = conversionId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{conversionId}")]
        public IActionResult GetPdfResult(string conversionId)
        {
            var filePath = Path.Combine(_tempDirectory, $"{conversionId}.pdf");

            if (System.IO.File.Exists(filePath))
            {
                var pdfBytes = System.IO.File.ReadAllBytes(filePath);
                return File(pdfBytes, "application/pdf", "output.pdf");
            }

            return NotFound("Conversion result not available yet. Please try again later.");
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        [AutomaticRetry(Attempts = 0)]
        public async Task ConvertAndSavePdf(string fileName, byte[] fileContent, string conversionId)
        {
            try
            {
                using (var stream = new MemoryStream(fileContent))
                {
                    var htmlContent = new StreamReader(stream).ReadToEnd();
                    var pdfBytes = await ConvertToPdf(htmlContent);
                    var filePath = Path.Combine(_tempDirectory, $"{conversionId}.pdf");

                    System.IO.File.WriteAllBytes(filePath, pdfBytes);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PDF conversion: {ex.Message}");
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        private async Task<byte[]> ConvertToPdf(string htmlContent)
        {
            using (var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe" }))
            using (var page = await browser.NewPageAsync())
            {
                await page.SetContentAsync(htmlContent);
                return await page.PdfDataAsync();
            }
        }
        [HttpGet("CheckStatus/{conversionId}")]
        public IActionResult CheckStatus(string conversionId)
        {
            var filePath = Path.Combine(_tempDirectory, $"{conversionId}.pdf");

            if (System.IO.File.Exists(filePath))
            {
                return Ok("Found");
            }

            return Ok("Not Found");
        }
        [HttpGet("DownloadResult/{conversionId}")]
        public IActionResult DownloadResult(string conversionId)
        {
            try
            {
                var filePath = Path.Combine(_tempDirectory, $"{conversionId}.pdf");

                if (System.IO.File.Exists(filePath))
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    var contentType = "application/octet-stream";

                    return File(fileStream, contentType, $"{conversionId}.pdf");
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        private static byte[] GetFileBytes(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
