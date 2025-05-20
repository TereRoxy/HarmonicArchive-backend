using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HarmonicArchiveBackend.Services
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var fileParameters = context.MethodInfo.GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IEnumerable<IFormFile>));

            if (fileParameters.Any())
            {
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = fileParameters.ToDictionary(
                                    p => p.Name ?? throw new InvalidOperationException("Parameter name cannot be null"),
                                    p => new OpenApiSchema { Type = "string", Format = "binary" }
                                ),

                                Required = fileParameters.Select(p => p.Name).ToHashSet()
                            }
                        }
                    }
                };
            }
        }
    }
}