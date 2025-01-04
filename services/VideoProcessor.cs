using System.Diagnostics;
using System;
using System.Text;  // Add this for StringBuilder
using System.Threading.Tasks;

public class VideoProcessor
{
    private readonly string _ffmpegPath;

    public VideoProcessor(string ffmpegPath)
    {
        _ffmpegPath = ffmpegPath;
    }
   

    public async Task<string> AddRealtimeDatetimeOverlayAsync(string inputVideoPath, string outputVideoPath, double latitude, double longitude)
    {
        try
        {
            // Path to the Arial font (update as needed)
            string fontFilePath = @"C:\Windows\Fonts\arial.ttf";

            // Get the current Unix timestamp (base time)
            long baseUnixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            long baseUnixTimeIST = baseUnixTime + (5 * 3600) + (30 * 60); // Add 5 hours 30 minutes
            string locationText = $"Lat: {latitude}, Lng: {longitude}";

            // Correct FFmpeg arguments

            string arguments = $"-i \"{inputVideoPath}\" " +
                   $"-vf \"drawtext=fontfile='C\\:\\\\Windows\\\\Fonts\\\\arial.ttf':" +
                   $"text='%{{pts\\:gmtime\\:{baseUnixTimeIST}}}':" +
                   $"x=20:y=50:fontsize=30:fontcolor=white:box=1:boxcolor=black@0.5," +
                   $"drawtext=fontfile='C\\:\\\\Windows\\\\Fonts\\\\arial.ttf':" +
                   $"text='Lat\\: {latitude}, Lng\\: {longitude}':" +
                   $"x=20:y=100:fontsize=30:fontcolor=white:box=1:boxcolor=black@0.5\" " +
                   $"-codec:a copy \"{outputVideoPath}\"";

            // Configure the process
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,  // Ensure ffmpeg is in PATH or provide full path
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Run FFmpeg process
            using var process = new Process { StartInfo = processStartInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, args) => outputBuilder.AppendLine(args.Data);
            process.ErrorDataReceived += (sender, args) => errorBuilder.AppendLine(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed: {errorBuilder}");
            }

            return outputVideoPath;
        }
        
        catch (Exception ex)
        {
            throw new Exception($"Error processing video: {ex.Message}");
        }
    }

}
