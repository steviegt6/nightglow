using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Nightglow.Common;

public class Downloader {
    private static HttpClient HttpClient { get; } = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task Download(string url, string destination, Func<long?, long, double?, bool> progressChanged) {
        using var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        var totalBytes = response.Content.Headers.ContentLength;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        var totalBytesRead = 0L;
        var readCount = 0L;
        var buffer = new byte[8192];
        var isMoreToRead = true;

        static double? calculatePercentage(long? totalDownloadSize, long totalBytesRead)
            => totalDownloadSize.HasValue ? Math.Round((double) totalBytesRead / totalDownloadSize.Value, 2) : null;

        using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        do {
            var bytesRead = await contentStream.ReadAsync(buffer);
            if (bytesRead == 0) {
                isMoreToRead = false;

                if (progressChanged(totalBytes, totalBytesRead, calculatePercentage(totalBytes, totalBytesRead)))
                    throw new OperationCanceledException();

                continue;
            }

            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));

            totalBytesRead += bytesRead;
            readCount++;

            if (readCount % 100 == 0) {
                if (progressChanged(totalBytes, totalBytesRead, calculatePercentage(totalBytes, totalBytesRead)))
                    throw new OperationCanceledException();
            }
        }
        while (isMoreToRead);
    }
}
