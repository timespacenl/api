using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TimeSpace.Shared.TypescriptGenerator;

namespace Timespace.Api.Application.Features.StartupJobs;

public class ApiDetailsExtractor(IApiDescriptionGroupCollectionProvider apiExplorer, IConfiguration configuration)
{
    public void Execute()
    {
        var endpoints = new List<EndpointDescription>();
        
        var apiDescriptionGroups = apiExplorer.ApiDescriptionGroups.Items;
        foreach (var apiDescriptionGroup in apiDescriptionGroups)
        {
            foreach (var apiDescription in apiDescriptionGroup.Items)
            {
                endpoints.Add(TransformApiDescription(apiDescription, apiDescriptionGroup.GroupName));
            }
        }

        var filePath = configuration.GetValue<string>("ApiDetailsGenerationPath");
        
        if(filePath == null)
            throw new Exception("ApiDetailsGenerationPath is not set in appsettings.json");
        
        var fileContent = JsonSerializer.Serialize(endpoints, new JsonSerializerOptions()
        {
            WriteIndented = true,
        });
        File.WriteAllText(filePath, fileContent);
    }
    private EndpointDescription TransformApiDescription(ApiDescription apiDescription, string? groupName)
    {
        if (apiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor)
            throw new Exception($"The action descriptor is not a controller action descriptor for path {apiDescription.RelativePath}.");
        
        var endpointDescription = new EndpointDescription()
        {
            RelativePath = apiDescription.RelativePath!,
            Version = groupName,
            ActionName = controllerActionDescriptor.MethodInfo.Name,
            HttpMethod = apiDescription.HttpMethod,
            ControllerTypeName = controllerActionDescriptor.ControllerTypeInfo.FullName!,
            //Parameters = paramsTest.Select(TransformApiParameter).ToList()
        };
        
        return endpointDescription;
    }
    
    private ParameterDescription TransformApiParameter(ApiParameterDescription apiParameterDescription)
    {
        return new ParameterDescription
        {
            Name = apiParameterDescription.ModelMetadata.PropertyName ?? apiParameterDescription.ModelMetadata.ParameterName ?? apiParameterDescription.Name,
            Source = GetSource(apiParameterDescription.Source)
        };
    }
    
    private ParameterSource GetSource (BindingSource bindingSource)
    {
        if (bindingSource == BindingSource.Query)
            return ParameterSource.Query;

        if (bindingSource == BindingSource.Path)
            return ParameterSource.Path;

        if (bindingSource == BindingSource.Form)
            return ParameterSource.Form;

        if (bindingSource == BindingSource.Body)
            return ParameterSource.Body;
        
        throw new ArgumentOutOfRangeException(bindingSource.ToString());
    }

}
