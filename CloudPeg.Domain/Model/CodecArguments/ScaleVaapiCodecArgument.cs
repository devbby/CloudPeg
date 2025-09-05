namespace CloudPeg.Domain.Model.CodecArguments;

public class ScaleVaapiCodecArgument : IScaleCodecArgument
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Type { get; set; } = "scale_vaapi";
    public string Argument { get; set; }

    public ScaleVaapiCodecArgument(int width, int height)
    {
        Width = width;
        Height = height;
        Argument = $"-vf hwupload,scale_vaapi=w={width}:h={height}:format=nv12";
    }

   
}