namespace YtDlpUi.UI.Services;

public interface IFileSystemLauncher
{
    bool TryOpenFile(string path);

    bool TryOpenLocation(string path);

    bool TryOpenUrl(string url);
}
