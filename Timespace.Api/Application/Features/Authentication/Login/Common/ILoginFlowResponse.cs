﻿namespace Timespace.Api.Application.Features.Authentication.Login.Common;

public interface ILoginFlowResponse
{
    public Guid FlowId { get; set; }
    public string NextStep { get; set; }
    public Instant ExpiresAt { get; set; }
    public string? SessionToken { get; set; }
    public List<string> NextStepAllowedMethods { get; set; }
}