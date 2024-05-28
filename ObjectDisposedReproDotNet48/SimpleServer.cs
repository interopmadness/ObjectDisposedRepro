using System;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;

namespace ObjectDisposedReproDotNet48
{
    internal static class SimpleServer
    {
        // Base64 representation of small docx file, see https://github.com/stiv-yakovenko/toptal-docx/blob/master/minimal.docx
        private static readonly string minimalDocxBase64 = "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";

        public static void StartServer(string urlPrefix, CancellationToken cancelToken)
            => new Thread(() =>
            {
                using (var listener = new HttpListener() { IgnoreWriteExceptions = true })
                {
                    listener.Prefixes.Add(urlPrefix);
                    listener.Start();
                    while (!cancelToken.IsCancellationRequested)
                    {
                        listener.GetContextAsync().ContinueWith(async (t) =>
                        {
                            try
                            {
                                if (cancelToken.IsCancellationRequested)
                                    return;
                                var ctx = await t;

                                using (ctx.Response)
                                {
                                    ctx.Response.SendChunked = true;
                                    ctx.Response.StatusCode = (int)HttpStatusCode.OK;
                                    ctx.Response.Headers["synprodid"] = "7";
                                    ctx.Response.Headers["synsetfileattr"] = "N";
                                    ctx.Response.Headers["Content-Type"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                                    ctx.Response.Headers["X-IsSafeFile"] = "True";

                                    using (var resultStream = GetFileStream(out var size, out var fileName))
                                    {
                                        ctx.Response.Headers["Content-Disposition"] = new ContentDispositionHeaderValue("attachment")
                                        {
                                            FileNameStar = fileName,
                                            Size = size
                                        }.ToString();
                                        await resultStream.CopyToAsync(ctx.Response.OutputStream, 81920, cancelToken);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }, cancelToken);
                    }
                    listener.Stop();
                }
            }).Start();

        private static Stream GetFileStream(out long fileSizeBytes, out string fileName)
        {
            var minimalDocxBytes = Convert.FromBase64String(minimalDocxBase64);
            fileSizeBytes = minimalDocxBytes.Length;
            fileName = "minimal.docx";
            return new MemoryStream(minimalDocxBytes);
        }
    }
}
