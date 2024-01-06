// // See https://aka.ms/new-console-template for more information
// using OcrService;

// var listener = new ExecuteOcrListener();
// listener.Listen();

using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OcrService;

class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services.AddMassTransit(x =>
                {
                    x.AddConsumer<ExecuteOcrCommandConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("ocr-queue", e =>
                        {
                            e.Durable = true;
                            e.ConfigureConsumer<ExecuteOcrCommandConsumer>(context);
                        });
                    });
                });

                //services.AddMassTransitHostedService();
            })
            .Build();

        await host.RunAsync();
    }
}
