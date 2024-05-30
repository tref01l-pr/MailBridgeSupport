using AutoMapper;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.DataAccess.SqlServer;

public class DataAccessMappingProfile : Profile
{
    public DataAccessMappingProfile()
    {
        CreateMap<UserEntity, User>().ReverseMap();
        CreateMap<SentMessageEntity, SentMessage>().ReverseMap();
        CreateMap<ReceivedMessageEntity, ReceivedMessage>().ReverseMap();
        CreateMap<SessionEntity, Session>().ReverseMap();
    }
}