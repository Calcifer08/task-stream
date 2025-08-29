using AutoMapper;
using TaskStream.Shared.Protos.Auth;
using TaskStream.Auth.Application.DTOs;

namespace TaskStream.Auth.API.Mapping;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        // gRPC -> DTO
        CreateMap<RegisterRequest, RegisterDto>();
        CreateMap<LoginRequest, LoginDto>();
        CreateMap<RefreshRequest, RefreshTokenDto>();

        // DTO -> gRPC
        CreateMap<AuthResultDto, AuthResponse>();
    }
}