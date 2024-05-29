#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0063 // 'using' statement can be simplified
#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;

namespace ObjectDisposedReproDotNet48
{
    static class Program
    {
        // Cancels client and server operations

        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        // UrlPrefix for the server
        private static string UrlPrefix => "http://+:18093/";

        // This property creates a valid hostname, given the UrlPrefix
        // E.g. https://+:18093/ --> https://myhost.contoso.com:18093/ if this machine's hostname+FQDN is mymachine.contoso.com
        // You can change how the hostname is determined in the method GetThisHostName()
        internal static Uri RequestBaseUrl
        {
            get
            {
                var wildcardUrlPrefixRegex = new Regex(@"^http[s]?:\/\/(\+|\*)");

                var urlHostNamePlaceHolder = "30fe441eb9694d3fa7b827402a6851de";
                var urlPrefixWithPlaceholder = wildcardUrlPrefixRegex.Replace(UrlPrefix, $"http://{urlHostNamePlaceHolder}");
                var builder = new UriBuilder(urlPrefixWithPlaceholder)
                {
                    Host = GetThisHostName()
                };

                return builder.Uri;
            }
        }

#if NET8_0_OR_GREATER
#nullable disable
#endif
        private static void OnConsoleCancel(object sender, ConsoleCancelEventArgs e)
        {
            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Console.WriteLine("\r\nAborting...");
                CancellationTokenSource.Cancel();
                Console.CancelKeyPress -= OnConsoleCancel;
            }
        }
#if NET8_0_OR_GREATER
#nullable restore
#endif

        static void Main()
        {
            int attempts = 0;

            Console.WriteLine("CTRL+C to abort\r\n");
            Console.CancelKeyPress += OnConsoleCancel;

            try
            {
                SimpleServer.StartServer(UrlPrefix, CancellationTokenSource.Token);

                while (!CancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (++attempts % 100 == 0)
                        Console.WriteLine($"Attempt {attempts:D5}...");

                    SendRequest();

//                    DumbClient.Go();

                    //if (attempts > 1)
                    //    break;
                }
                CancellationTokenSource.Token.WaitHandle.WaitOne();
            }
            catch (Exception ex)
            {
                CancellationTokenSource.Cancel();
                Environment.ExitCode = 1;
                Console.WriteLine($"\r\n{ex}");
                Console.WriteLine($"\r\nAttempts: {attempts}");
            }
        }

        private static void SendRequest()
        {
            var serverInstance = new SimpleClient(RequestBaseUrl, CancellationTokenSource.Token);
            using (var responseStream = serverInstance.GetContentStream2()) { }
        }

        // Returns hostname of this machine. Change accordingly, if insufficient for your setup
        private static string GetThisHostName()
        {
            // Replace this with some different hostname if it doesn't work out for you
            // If https://, make sure hostname matches SSL certificate (subjectAlternativeName)
            // Also make sure that returned hostname is compatible with configured UrlAcl (if not running process as admin)
            return $"{Dns.GetHostName()}.{IPGlobalProperties.GetIPGlobalProperties().DomainName}".TrimEnd('.');
        }
    }
}
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
#pragma warning restore IDE0090 // Use 'new(...)'
#pragma warning restore IDE0063 // 'using' statement can be simplified
#pragma warning restore IDE0079 // Remove unnecessary suppression