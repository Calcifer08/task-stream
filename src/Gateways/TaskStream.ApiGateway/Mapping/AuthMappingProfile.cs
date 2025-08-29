using AutoMapper;
using TaskStream.ApiGateway.Models.Auth.Requests;
using GrpcContracts = TaskStream.Shared.Protos.Auth;

namespace TaskStream.ApiGateway.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // gRPC -> REST
        CreateMap<RegisterRequest, GrpcContracts.RegisterRequest>();
        CreateMap<LoginRequest, GrpcContracts.LoginRequest>();
        CreateMap<TokenRefreshRequest, GrpcContracts.RefreshRequest>();

        // REST -> gRPC
        CreateMap<GrpcContracts.AuthResponse, AuthSuccessResponse>();
    }
}