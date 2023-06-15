namespace Timespace.Api.Application.Features.ExternalSourceGeneration.Templates;

public static class TypescriptApiClientTemplates
{
    public const string ApiClientHeader = @"
        type ApiResponse<T> = {
	        data: T | null;
	        error: string | null;
        }

        export type FetchType = typeof fetch;
    ";
    
    public const string ApiClientFunction = """
    export async function getRegistrationFlow(fetch: FetchType, [[params]]): Promise<ApiResponse<GetLoginFlowResponse>> {
        let response = await fetch(`${PUBLIC_BASE_URL}/auth/registration?flow=${flowId}`);

        if(response.ok){
            let data = await response.json() as GetLoginFlowResponse;
        
            return {
                data: data,
                error: null
            };
        } else {
            let data = await response.json() as ProblemDetails;

            return {
                data: null,
                error: data.type
            };
        }
    }
    """;
}