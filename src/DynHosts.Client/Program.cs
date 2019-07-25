using McMaster.Extensions.CommandLineUtils;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DynHosts.Client
{
    class Program
    {
        static readonly HttpClient s_httpClient = new HttpClient();

        static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "dhclient",
                Description = "DynHosts Client"
            };

            app.HelpOption("-?|-h|--help");

            var remoteHostOption = app.Option("-r|--remote-host <URL>",
                    "The URL of the DynHosts Server to connect to.",
                    CommandOptionType.SingleValue);

            var hostnameOption = app.Option("-h|--hostname <NAMES>",
                    "The hostname(s) to modify in the HOSTS file.",
                    CommandOptionType.MultipleValue);

            var ipsOption = app.Option("-i|--ip <IP>",
                    "The IP address to set the hostname value(s) to in the HOSTS file.",
                    CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                if (remoteHostOption.HasValue() && hostnameOption.HasValue() && ipsOption.HasValue())
                {
                    s_httpClient.BaseAddress = new Uri(remoteHostOption.Value());

                    string remoteHost = remoteHostOption.Value();
                    string hostname = hostnameOption.Value();

                    foreach (var ip in ipsOption.Values)
                    {
                        Console.Write($"Requesting server at {remoteHost} update host \"{hostname}\" to \"{ip}\"... ");
                        var postContent = new StringContent("{" + "'ipAddress':'" + ip + "'}", Encoding.UTF8, "application/json");

                        var contentAsString = await postContent.ReadAsStringAsync();
                        Console.WriteLine($"\r\n" + contentAsString);
                        Newtonsoft.Json.JsonConvert.DeserializeObject(contentAsString);

                        using (var response = await s_httpClient.PutAsync("/api/hosts/" + hostname, postContent))
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
                }
                else
                {
                    app.ShowHint();
                }

                return 0;
            });

            return app.Execute(args);
        }
    }
}
