using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;

namespace DynHosts.Client
{
    class Program
    {
        static readonly HttpClient s_httpClient = new HttpClient();

        public static async Task<int> Main(string[] args)
        {
            using (
                var app = new CommandLineApplication
                {
                    Name = "dhclient",
                    Description = "DynHosts Client"
                }
            )
            {
                app.HelpOption("-?|-h|--help");

                var remoteHostOption = app.Option("-r|--remote-host <URL>",
                        "The URL of the DynHosts Server to connect to.",
                        CommandOptionType.SingleValue);

                var hostnameOption = app.Option("-h|--hostname <NAME>",
                        "The hostname to modify in the HOSTS file.",
                        CommandOptionType.SingleValue);

                var ipsOption = app.Option("-i|--ip <IP>",
                        "The IP address(es) to set the hostname value to in the HOSTS file. Specify multiple times for multiple IP adresses e.g. --ip 127.0.0.1 --ip ::1",
                        CommandOptionType.MultipleValue);

                app.OnExecuteAsync(async (cancellationToken) =>
                {
                    if (remoteHostOption.HasValue() && hostnameOption.HasValue() && ipsOption.HasValue())
                    {
                        s_httpClient.BaseAddress = new Uri(remoteHostOption.Value());
                        s_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                        string remoteHost = remoteHostOption.Value();
                        string hostname = hostnameOption.Value();

                        Console.Write($"Requesting server at {remoteHost} update host \"{hostname}\"... ");
                        var postContent = new StringContent(JsonConvert.SerializeObject(new { host = hostname, ipAddresses = ipsOption.Values.ToArray() }), Encoding.UTF8, "application/json");

                        using (
                            var response = await Observable.FromAsync(async () => await s_httpClient.PutAsync("/api/hosts/" + hostname, postContent))
                                .Select(r =>
                                {
                                    if (!r.IsSuccessStatusCode)
                                    {
                                        throw new HttpListenerException((int)r.StatusCode, r.ReasonPhrase);
                                    }
                                    return r;
                                })
                                .Retry(3)
                                .Catch((Func<Exception, IObservable<HttpResponseMessage>>)(ex =>
                                {
                                    if (ex is HttpListenerException hle)
                                    {
                                        return Observable.Return(new HttpResponseMessage((HttpStatusCode)hle.ErrorCode) { ReasonPhrase = hle.Message });
                                    }

                                    throw ex;
                                }))
                        )
                        {
                            if (new[] { HttpStatusCode.Created, HttpStatusCode.OK, HttpStatusCode.NoContent }.Contains(response.StatusCode))
                            {
                                Console.WriteLine("OK!");
                            }
                            else
                            {
                                Console.WriteLine("Failed.");
                                Console.WriteLine($"{(int)response.StatusCode} - {response.ReasonPhrase}");

                                string responseContent = await response.Content.ReadAsStringAsync();
                                Console.WriteLine(responseContent);
                            }
                        }
                        Console.WriteLine();
                    }
                    else
                    {
                        app.ShowHint();
                    }

                    return 0;
                });

                return await app.ExecuteAsync(args);
            }
        }
    }
}
