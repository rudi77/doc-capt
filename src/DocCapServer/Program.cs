using DocCapServer.Consumers;
using Elsa.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.EntityFrameworkCore.Extensions;
using MassTransit;
using DocCapServer.Workflows;
using DocCapServer.Activities;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

// Add Elsa services.
services.AddElsa(elsa => 
    {
        // Configure management feature to use EF Core.
        elsa.UseWorkflowManagement(management => { 
            management.UseEntityFrameworkCore(ef => ef.UseSqlite()); });

        // Default Identity features for authentication/authorization.
        elsa.UseIdentity(identity =>
        {
            identity.TokenOptions = options => options.SigningKey = "sufficiently-large-secret-signing-key"; // This key needs to be at least 256 bits long.
            identity.UseAdminUserProvider();
        });
        
        // Configure ASP.NET authentication/authorization.
        elsa.UseDefaultAuthentication(auth => auth.UseAdminApiKey());        
     
    
        elsa.UseWorkflowRuntime(configure => configure.UseMassTransitDispatcher());
        elsa.AddWorkflow<DocumentCaptureHttpWorkflow>();        
        elsa.AddActivity<ExecuteOcrActivity>();

        // Enable Elsa HTTP module (for HTTP related activities). 
        elsa.UseHttp();
        elsa.UseWorkflowsApi();

        
        elsa.UseMassTransit(massTransit => 
        {            
            massTransit.AddConsumer<OcrCompletedConsumer>();
            massTransit.UseRabbitMq("amqp://guest:guest@localhost:5672");
        });
    }
);

// Configure CORS to allow designer app hosted on a different origin to invoke the APIs.
builder.Services.AddCors(cors => cors
    .AddDefaultPolicy(policy => policy
        .AllowAnyOrigin() // For demo purposes only. Use a specific origin instead.
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithExposedHeaders("x-elsa-workflow-instance-id"))); // Required for Elsa Studio in order to support running workflows from the designer. Alternatively, you can use the `*` wildcard to expose all headers.

// Configure middleware pipeline.
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

// Configure web application's middleware pipeline.
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflows();
app.UseWorkflowsApi();

app.Run();