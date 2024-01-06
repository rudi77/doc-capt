using DocCapServer.Activities;
using DocCapServer.Consumers;
using DocCapServer.Workflows;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.Extensions;
using Elsa.Workflows.Middleware.Activities;
using Elsa.Workflows.Middleware.Workflows;
using Elsa.Workflows.Runtime.Contracts;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// services.AddMassTransit(x =>
// {
//     x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
//     {
//         cfg.ReceiveEndpoint("ocr-finished-queue", e =>
//         {
//             e.Consumer<OcrCompletedConsumer>(() => new OcrCompletedConsumer(null!));
//         });
//     }));
// });

// Add Elsa services.
services.AddElsa(elsa => 
    {
        // Configure management feature to use EF Core.
        elsa.UseWorkflowManagement(management => { management.UseEntityFrameworkCore(ef => ef.UseSqlite()); });
        elsa.UseWorkflows(workflows =>
        {
            // Configure workflow execution pipeline to handle workflow contexts.
            workflows.WithWorkflowExecutionPipeline(pipeline => pipeline
                .Reset()
                .UsePersistentVariables()
                .UseBookmarkPersistence()
                .UseWorkflowExecutionLogPersistence()
                .UseWorkflowExecutionLogPersistence()
                .UseActivityExecutionLogPersistence()
                .UseDefaultActivityScheduler()
            );
            
            // Configure activity execution pipeline to handle workflow contexts.
            workflows.WithActivityExecutionPipeline(pipeline => pipeline
                .Reset()
                .UseExecutionLogging()
                .UseBackgroundActivityInvoker()
            );
        })
        .UseWorkflowManagement(management =>
        {
            // Use EF core for workflow definitions and instances.
            management.UseWorkflowInstances(m => m.UseEntityFrameworkCore(ef => ef.UseSqlite()));
            management.UseEntityFrameworkCore(m => m.UseSqlite());
        });        

        // Default Identity features for authentication/authorization.
        elsa.UseIdentity(identity =>
        {
            identity.TokenOptions = options => options.SigningKey = "sufficiently-large-secret-signing-key"; // This key needs to be at least 256 bits long.
            identity.UseAdminUserProvider();
        });
        
        // Configure ASP.NET authentication/authorization.
        elsa.UseDefaultAuthentication(auth => auth.UseAdminApiKey());
        

        //elsa.UseWorkflowRuntime(configure => configure.UseMassTransitDispatcher());
        elsa.AddWorkflow<DocumentCaptureHttpWorkflow>();        
        elsa.AddActivity<ExecuteOcrActivity>();

        // Enable Elsa HTTP module (for HTTP related activities). 
        elsa.UseHttp();
        elsa.UseWorkflowsApi();

        elsa.UseMassTransit(massTransit => 
        {            
            massTransit.AddConsumer<OcrCompletedConsumer>();
            massTransit.UseRabbitMq(
                "amqp://guest:guest@localhost:5672",             
                rabbitMqFeature => rabbitMqFeature.ConfigureServiceBus = bus => 
                { 
                    bus.PrefetchCount = 1;
                    bus.Durable = false;
                    bus.AutoDelete = false;
                    bus.ConcurrentMessageLimit = 32;
                    // bus.ReceiveEndpoint(
                    //     "ocr-completed-queue", 
                    //     e => ConsumerExtensions.Consumer<OcrCompletedConsumer>(
                    //         e, 
                    //     () => new OcrCompletedConsumer(null!)));                 
                });
        });
    }
);



// Configure middleware pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();




// Add Elsa HTTP middleware (to handle requests mapped to HTTP Endpoint activities)
app.UseWorkflows();
app.UseWorkflowsApi();
app.UseAuthentication();
app.UseAuthorization();
// Start accepting requests.
app.Run();