// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using PollyRetrySample;

Console.WriteLine("Program Start");

using IHost host = CreateHostBuilder(args).Build();

var serviceProver = host.Services;

var sampleService = serviceProver.GetService<ISampleService>();
var result = await sampleService.DoSomething();

Console.WriteLine($"Result = {result}");

await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.SetBasePath(Directory.GetCurrentDirectory());
        configHost.AddCommandLine(args);
    }).ConfigureServices((context, collection) =>
       collection
           .AddHttpClient<ISampleService, SampleService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
            })
        .AddPolicyHandler(GetRetryPolicy())
        );
            
            
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}            