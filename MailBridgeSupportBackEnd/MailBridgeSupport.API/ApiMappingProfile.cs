using AutoMapper;
using MailBridgeSupport.API.Contracts;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.API;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        CreateMap<User, GetUserResponse>();
        CreateMap<ImapMessage, GetMessageResponse>();
    }
}