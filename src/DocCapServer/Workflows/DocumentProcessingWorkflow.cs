using Elsa.Extensions;
using Elsa.Http;
using Elsa.Workflows;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using DocCap.Contracts;
using DocCapServer.Activities;

namespace DocCapServer.Workflows;

public class DocumentCaptureHttpWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        // stores the files uploaded to the HTTP endpoint
        var files = builder.WithVariable<IFormFile[]>("Document", null).WithMemoryStorage();

        builder.Root = new Sequence
        {
            Activities =
            {
                new HttpEndpoint
                {
                    Path = new("/document-capture"),
                    SupportedMethods = new(new[] { HttpMethods.Post }),
                    CanStartWorkflow = true,
                    Files = new (files),                    
                },
                new ExecuteOcrActivity
                {
                    DocumentContent = new(context => {
                        var uploadedFiles = files.Get(context);
                        
                        // Check if there is at least one file
                        if (uploadedFiles != null && uploadedFiles.Length > 0)
                        {
                            IFormFile file = uploadedFiles[0]; // Get the first file

                            using var memoryStream = new MemoryStream();
                            file.CopyTo(memoryStream);
                            byte[] fileBytes = memoryStream.ToArray();                            
                            return fileBytes;
                        }

                        return null;                        
                    })
                },
                new OcrCompletedActivity
                {
                    MessageType = typeof(OcrCompleted)
                },                
            }
        };
    }
}