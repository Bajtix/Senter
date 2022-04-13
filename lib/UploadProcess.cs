using System.ComponentModel;
using System.IO.Compression;
using System.Net;

namespace Senter.Communication;

public class UploadProcess {
    public Uploadable uploadable;
    public long totalSize;
    public long sentBytes;

    public Action<AsyncCompletedEventArgs> onUploadFinish;
    public Action<UploadProgressChangedEventArgs> onUploadProgress;

    private string localPath, htoken, happ, hversion, hplatform, targetZip;


    public UploadProcess(Uploadable uploads) {
        this.uploadable = uploads;
        this.localPath = uploadable.folderPath;
        this.happ = uploadable.app.id.ToString();
        this.htoken = DevPackage.currentToken.GetToken();
        this.hversion = uploadable.version.version;
        this.hplatform = uploadable.platform.ToString();

        targetZip = Path.Combine(SenterSettings.tmpPath, $"{uploads.app.name}_{uploads.version.version}_{uploads.platform}.zip");

        if (Directory.GetFiles(localPath).Length <= 0) {
            Console.WriteLine($"directory {localPath} is empty");
        }
        if (File.Exists(targetZip)) File.Delete(targetZip);
        var t = new Task(() => ZipFile.CreateFromDirectory(localPath, targetZip));
        t.Start();
        t.ContinueWith((w) => {
            using var wc = new WebClient();
            wc.UploadProgressChanged += ProgressChanged;
            wc.UploadFileCompleted += UploadFinished;

            wc.Headers.Add("token", htoken);
            wc.Headers.Add("app", happ);
            wc.Headers.Add("version", hversion);
            wc.Headers.Add("platform", hplatform);

            wc.UploadFileAsync(new Uri(SenterSettings.host + "upload.php"), targetZip);
            //var r = wc.UploadFile(new Uri(SenterSettings.host + "upload.php"), targetZip);
            //Console.WriteLine(System.Text.Encoding.UTF8.GetString(r));
        });


    }

    private void UploadFinished(object sender, UploadFileCompletedEventArgs args) {
        if (args.Cancelled) {
            sentBytes = -1;
            return;
        }
        Console.WriteLine(args.Error);
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(args.Result));

        if (onUploadFinish != null) onUploadFinish.Invoke(args);

        if (File.Exists(targetZip)) File.Delete(targetZip);
    }

    private void ProgressChanged(object obj, UploadProgressChangedEventArgs args) {
        sentBytes = args.BytesReceived;
        totalSize = args.TotalBytesToReceive;

        if (onUploadProgress != null) onUploadProgress.Invoke(args);
    }
}