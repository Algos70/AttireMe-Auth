using AuthenticationService.DTOs.Requests;
using AuthenticationService.DTOs.Responses;
using AuthenticationService.Entities;
using AutoMapper;

namespace AuthenticationService.Mappings;

public class GeneralProfile : Profile
{
    public GeneralProfile()
    {
        // Add mappings for Creator and AppUser if needed
        // Example:
        // CreateMap<Creator, SomeCreatorResponse>();
        // CreateMap<UpdateCreatorRequest, Creator>();
        // CreateMap<AppUser, SomeUserResponse>();
        // CreateMap<UpdateUserRequest, AppUser>();
    }
}