namespace API.Core.Options;

public sealed class ServerOptions
{
    public string Domain { get; set; } = string.Empty;
    public string TokenRecoveryPath { get; set; } = string.Empty;
}