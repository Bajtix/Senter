using System.ComponentModel;
using System.Net;
using System.IO.Compression;

namespace Senter.Communication;


public enum DownloadStage {
    download,
    install,
    complete,
    error
}

public class DownloadProcess {
    public Downloadable downloadable;
    public long totalSize;
    public long receivedBytes;

    public Action<AsyncCompletedEventArgs> onDownloadFinish;
    public Action<DownloadProgressChangedEventArgs> onDownloadProgress;
    public Action onInstallFinish;
    public DownloadStage stage;

    private string url, dlPath, installPath;


    public DownloadProcess(Downloadable downloads) {
        this.downloadable = downloads;
        this.url = downloads.link;
        this.dlPath = downloads.potentialInstallPath;

        Directory.CreateDirectory(SenterSettings.tmpPath);

        if (File.Exists(dlPath)) File.Delete(dlPath);

        using (var wc = new WebClient()) {
            wc.DownloadProgressChanged += ProgressChanged;
            wc.DownloadFileCompleted += DownloadFinished;

            wc.DownloadFileAsync(new Uri(url), dlPath);
            stage = DownloadStage.download;
        }


    }

    private void DownloadFinished(object sender, AsyncCompletedEventArgs args) {
        if (args.Cancelled) {
            receivedBytes = -1;
            return;
        }

        Install();

        if (onDownloadFinish != null) onDownloadFinish.Invoke(args);
    }

    private void ProgressChanged(object obj, DownloadProgressChangedEventArgs args) {
        receivedBytes = args.BytesReceived;
        totalSize = args.TotalBytesToReceive;

        if (onDownloadProgress != null) onDownloadProgress.Invoke(args);
    }

    private void Install() {
        installPath = Management.GetInstallationPath(downloadable);
        if (Directory.Exists(installPath)) Directory.Delete(installPath, true); // unzipping needs an empty directory;
        Directory.CreateDirectory(installPath);

        var task = new Task(Extract);
        task.ContinueWith((a) => onInstallFinish.Invoke());
        task.Start();
    }

    private void Extract() {
        stage = DownloadStage.install;
        ZipFile.ExtractToDirectory(dlPath, installPath);
        File.Delete(dlPath);
        Management.MarkInstalled(downloadable);
        stage = DownloadStage.complete;
    }
}