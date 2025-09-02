using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CloudPeg.Application.Utility;

public static class FfmpegCodecUtility
{
    /// <summary>
    /// Represents a single FFmpeg codec (encoder or decoder).
    /// </summary>
    public class Codec
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAudio { get; set; }
        public bool IsVideo { get; set; }
        public bool IsSubtitle { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Description: {Description}";
        }
    }
 
    private static string RunFfmpegCommand(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running FFmpeg command: {ex.Message}");
            return string.Empty;
        }
    }

    private static List<Codec> ParseFfmpegOutput(string output)
    {
        var codecs = new List<Codec>();
        var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        bool parsingStarted = false;

        foreach (var line in lines)
        {
            if (!parsingStarted)
            {
                if (line.Trim() == "------")
                {
                    parsingStarted = true;
                }
                continue;
            }

            string trimmedLine = line.TrimStart();
            if (trimmedLine.Length < 7 || trimmedLine[6] != ' ')
            {
                continue;
            }

            // Example line format to parse:
            // "DE..C. aac                  AAC (Advanced Audio Coding) (decoders: aac aac_fixed )"
            // " V.... libx264              libx264 H.264 / AVC / MPEG-4 AVC / MPEG-4 part 10 (encoders)"
            var parts = trimmedLine.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                continue;
            }

            string flags = parts[0];
            string description = parts[1];
            string name = description.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            // Clean up the name if it contains flags
            if (flags.Length > 2)
            {
                description = description.Substring(flags.Length).Trim();
                name = description.Split(' ')[0];
            }
            
            // Further refine the name and description. The FFmpeg output can be tricky.
            // Let's use a regex to capture the first word and the rest of the description.
            var match = Regex.Match(trimmedLine, @"^.{6}\s+([^\s]+)\s+(.*)$");
            if (match.Success)
            {
                name = match.Groups[1].Value;
                description = match.Groups[2].Value.Trim();
            }

            codecs.Add(new Codec
            {
                Name = name,
                Description = description,
                IsAudio = flags.Contains("A", StringComparison.OrdinalIgnoreCase),
                IsVideo = flags.Contains("V", StringComparison.OrdinalIgnoreCase),
                IsSubtitle = flags.Contains("S", StringComparison.OrdinalIgnoreCase),
            });
        }
        return codecs;
    }
    
    public static bool IsCodecActuallySupported(Codec codec)
    {
        string arguments = string.Empty;
        if (codec.IsVideo)
        {
            // Use the testsrc filter to generate a dummy video stream.
            // The -f null - command discards the output, so no file is created.
            arguments = $"-f lavfi -i testsrc=d=1 -vcodec {codec.Name} -f null -";
        }
        else if (codec.IsAudio)
        {
            // Use the anullsrc filter to generate a dummy audio stream.
            arguments = $"-f lavfi -i anullsrc=d=1 -acodec {codec.Name} -f null -";
        }
        else
        {
            // Subtitles are assumed to be supported for now and will be handled later.
            return true;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
                // A successful test will have an exit code of 0.
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error testing codec '{codec.Name}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of all supported FFmpeg encoders.
    /// </summary>
    /// <returns>A list of Codec objects representing supported encoders.</returns>
    public static List<Codec> GetSupportedEncoders()
    {
        string output = RunFfmpegCommand("-encoders");
        return ParseFfmpegOutput(output);
    }

    /// <summary>
    /// Gets a list of all supported FFmpeg decoders.
    /// </summary>
    /// <returns>A list of Codec objects representing supported decoders.</returns>
    public static List<Codec> GetSupportedDecoders()
    {
        string output = RunFfmpegCommand("-decoders");
        return ParseFfmpegOutput(output);
    }
}