namespace CloudPeg.Domain.Model;

public class ScaleQsvCodecArgument : IScaleCodecArgument
{
    public ScaleQsvCodecArgument(int width, int height)
    {
        Width = width;
        Height = height;
        Argument = $"-vf hwupload,scale_qsv=w={width}:h={height},format=nv12";
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public string GetScalelessArgument()
    {
        return   $"-vf hwupload,format=nv12";
         
    }

    public string Type { get; set; } = "scale_qsv";
    public string Argument { get; set; }
}