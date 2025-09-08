namespace CloudPeg.Domain.Model.CodecArguments;

public class GenericCodecArgument : ICodecArgument
{
    public GenericCodecArgument(string argument)
    {
        Argument = argument;
    }

    public string Type { get; set; } = "generic";
    public string Argument { get; set; }
}