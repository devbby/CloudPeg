using System.Text.Json.Serialization;
using CloudPeg.Domain.Model.CodecArguments;

namespace CloudPeg.Domain.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GenericCodecArgument), "generic")]
[JsonDerivedType(typeof(ScaleVaapiCodecArgument), "scale_vaapi")]
[JsonDerivedType(typeof(ScaleQsvCodecArgument), "scale_qsv")]
public interface ICodecArgument
{
    public string Type { get; set; }  
    public string Argument { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ScaleVaapiCodecArgument), "scale_vaapi")]
[JsonDerivedType(typeof(ScaleQsvCodecArgument), "scale_qsv")]
public interface IScaleCodecArgument : ICodecArgument
{
    public int Width { get; set; }

    public int Height { get; set; }
    string GetScalelessArgument();
}