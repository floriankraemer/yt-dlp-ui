namespace YtDlpUi.Core.Models;

public sealed class BinaryInstallResult
{
    public bool IsSuccess { get; init; }
    public string? InstalledPath { get; init; }
    public string? Error { get; init; }

    public static BinaryInstallResult Success(string path) =>
        new() { IsSuccess = true, InstalledPath = path };

    public static BinaryInstallResult Failure(string error) =>
        new() { IsSuccess = false, Error = error };
}
