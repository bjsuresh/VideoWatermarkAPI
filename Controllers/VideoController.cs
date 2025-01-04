using Microsoft.AspNetCore.Mvc; // For ControllerBase, IActionResult, and attributes
using System.IO; // For File operations
using System.Threading.Tasks; // For async methods

[Route("[controller]")]

public class VideoController : ControllerBase
{
    private readonly VideoProcessor _videoProcessor;

    public VideoController()
    {
        // Update this path to point to your FFmpeg executable
        // string ffmpegPath = @"C:\Users\Admin\ffmpeg\bin\ffmpeg.exe";
        string ffmpegPath = @"C:\Users\Suresh\FFMPEG\bin\ffmpeg.exe";
        _videoProcessor = new VideoProcessor(ffmpegPath);
    }
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("API is working!");
    }
    [HttpPost("add-watermark")]
    public async Task<IActionResult> AddWatermark(IFormFile videoFile, [FromForm] double latitude, [FromForm] double longitude)
    {
        if (videoFile == null || videoFile.Length == 0)
        {
            return BadRequest("No video file provided.");
        }

        if (latitude == 0 || longitude == 0)
        {
            return BadRequest("Latitude and longitude must be provided.");
        }

        if (_videoProcessor == null)
        {
            throw new Exception("_videoProcessor is null. Ensure it is initialized correctly.");
        }

        string tempPath = Path.GetTempPath();
        string inputVideoPath = Path.Combine(tempPath, "videoFile.mp4");
        string outputVideoPath = Path.Combine(tempPath, $"watermark_{videoFile.FileName}");

        try
        {
            // Save the uploaded video file
            using (var stream = new FileStream(inputVideoPath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            // Validate input file creation
            if (!System.IO.File.Exists(inputVideoPath))
            {
                throw new Exception("Input video file was not created successfully.");
            }

            // Add watermark
            string result = await _videoProcessor.AddRealtimeDatetimeOverlayAsync(inputVideoPath, outputVideoPath, latitude, longitude);
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("Video processing failed. Output path is null or empty.");
            }
            Console.WriteLine($"Watermarked video saved at: {result},{outputVideoPath}");

            // Return processed video
            var videoBytes = await System.IO.File.ReadAllBytesAsync(result);
            return File(videoBytes, "video/mp4", Path.GetFileName(outputVideoPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return BadRequest(ex.Message);
        }
        finally
        {
            // Clean up temporary files
            if (System.IO.File.Exists(inputVideoPath)) System.IO.File.Delete(inputVideoPath);
            if (System.IO.File.Exists(outputVideoPath)) System.IO.File.Delete(outputVideoPath);
        }
    }

    // [HttpPost("add-watermark")]
    // public async Task<IActionResult> AddWatermark(IFormFile videoFile, [FromForm] double latitude, [FromForm] double longitude)
    // {
    //     string tempPath = Path.GetTempPath();
    //     string inputVideoPath = Path.Combine(tempPath, "videoFile");
    //     // string watermarkPath = Path.Combine(tempPath, watermarkImage.FileName);
    //     string outputVideoPath = Path.Combine(tempPath, "watermark_Video.mp4");
        
    //     try
    //     {
    //         // Save uploaded files
    //         using (var stream = new FileStream(inputVideoPath, FileMode.Create))
    //         {
    //             await videoFile.CopyToAsync(stream);
    //         }
         
    //         string result = await _videoProcessor.AddRealtimeDatetimeOverlayAsync(inputVideoPath, outputVideoPath, latitude, longitude);
    //         Console.WriteLine($"Watermarked video saved at: {result},{outputVideoPath}");
    //         // Return processed video
    //         var videoBytes = await System.IO.File.ReadAllBytesAsync(result);
    //         return File(videoBytes, "video/mp4", Path.GetFileName(outputVideoPath));
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error: {ex.Message}");
    //         return BadRequest(ex.Message);
    //     }
    //     finally
    //     {
    //         // Clean up temporary files
    //         // if (System.IO.File.Exists(inputVideoPath)) System.IO.File.Delete(inputVideoPath);
    //         // if (System.IO.File.Exists(watermarkPath)) System.IO.File.Delete(watermarkPath);
    //         if (System.IO.File.Exists(outputVideoPath)) System.IO.File.Delete(outputVideoPath);
    //     }
    // }
}
