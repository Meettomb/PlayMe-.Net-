using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Main_Project.Pages
{
    public class StreamVideoModel : PageModel
    {
        private readonly string _videoFolder = "wwwroot/videos/";

        public IActionResult OnGet(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var filePath = Path.Combine(_videoFolder, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Video file not found.");
            }

            var fileLength = new FileInfo(filePath).Length;

            // Check if the request includes a Range header
            if (Request.Headers.ContainsKey("Range"))
            {
                var rangeHeader = Request.Headers["Range"].ToString();
                if (rangeHeader.StartsWith("bytes="))
                {
                    var range = rangeHeader.Substring(6).Split('-');
                    long start = long.Parse(range[0]);
                    long end = range.Length > 1 && !string.IsNullOrEmpty(range[1])
                        ? long.Parse(range[1])
                        : fileLength - 1;

                    if (start >= 0 && end >= start && end < fileLength)
                    {
                        Response.StatusCode = 206; // Partial Content
                        Response.Headers.Add("Content-Range", $"bytes {start}-{end}/{fileLength}");
                        Response.Headers.Add("Accept-Ranges", "bytes");
                        Response.ContentLength = end - start + 1;

                        // Stream the specified range
                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                        fileStream.Seek(start, SeekOrigin.Begin);
                        var buffer = new byte[end - start + 1];
                        fileStream.Read(buffer, 0, buffer.Length);

                        return File(new MemoryStream(buffer), "video/mp4");
                    }
                }
            }

            // Serve the entire file if no Range header is present
            Response.Headers.Add("Accept-Ranges", "bytes");
            return File(System.IO.File.OpenRead(filePath), "video/mp4");
        }
    }
}
