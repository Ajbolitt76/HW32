using System.Security.Claims;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using HW32.Grpc;
using Microsoft.AspNetCore.Authorization;

namespace HW32.Services;

public class ProtectedDemoService : Grpc.ProtectedDemo.ProtectedDemoBase
{
    private readonly JwtService _jwtService;

    public ProtectedDemoService(JwtService jwtService)
    {
        _jwtService = jwtService;
    }
    
    public override Task<Public.Types.Response> Public(Public.Types.Request request, ServerCallContext context)
    {
        return Task.FromResult<Public.Types.Response>(new()
        {
            Token = _jwtService.CreateToken(request.Name)
        });
    }

    [Authorize]
    public override Task<PrivateResponse> Private(Empty request, ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        return Task.FromResult<PrivateResponse>(new()
        {
            Name = user.FindAll(ClaimTypes.Name).FirstOrDefault()?.Value ?? "NaN",
            Key = "SECRET_TOKEN"
        });
    }
}