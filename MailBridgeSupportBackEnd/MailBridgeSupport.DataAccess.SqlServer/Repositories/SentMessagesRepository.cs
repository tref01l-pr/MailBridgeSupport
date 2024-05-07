using AutoMapper;
using CSharpFunctionalExtensions;
using MailBridgeSupport.API;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MailBridgeSupport.DataAccess.SqlServer.Repositories;

public class SentMessagesRepository : ISentMessagesRepository
{
    private readonly MailBridgeSupportDbContext _context;
    private readonly IMapper _mapper;

    public SentMessagesRepository(MailBridgeSupportDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<Result<int>> CreateAsync(SentMessage sentMessage)
    {
        var sentMessageEntity = _mapper.Map<SentMessage, SentMessageEntity>(sentMessage);
        await _context.SentMessages.AddAsync(sentMessageEntity);
        var result =  await SaveAsync();
        if (!result.Value)
        {
            return Result.Failure<int>("Something went wrong during save message");
        }

        return sentMessageEntity.Id;
    }

    public async Task<Result<SentMessage[]>> GetByEemployeeIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<SentMessage[]>($"{nameof(userId)} is not valid");
        }

        var employeeSentMessages = await _context.SentMessages
            .TagWith("Get by employee sent messages")
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToArrayAsync();

        return _mapper.Map<SentMessageEntity[], SentMessage[]>(employeeSentMessages);
    }

    public async Task<Result<SentMessage[]>> GetByRequesterEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<SentMessage[]>($"{nameof(email)} could not be empty or with spaces");
        }

        var questionEmailSentMessages = await _context.SentMessages
            .TagWith("Get by question email sent messages")
            .AsNoTracking()
            .Where(x => x.To == email)
            .ToArrayAsync();

        return _mapper.Map<SentMessageEntity[], SentMessage[]>(questionEmailSentMessages);
    }

    public async Task<Result<SentMessage[]>> GetLastSentMessagesAsync()
    {
        var lastMessages = await _context.SentMessages
            .TagWith("Get last sent messages by question email")
            .AsNoTracking() 
            .GroupBy(m => m.To) 
            .Select(g => g.OrderByDescending(m => m.Date).FirstOrDefault()) // Выбираем последнее сообщение из каждой группы
            .ToArrayAsync();

        return _mapper.Map<SentMessageEntity[], SentMessage[]>(lastMessages);
    }

    public async Task<Result<bool>> SaveAsync() => await _context.SaveChangesAsync() > 0;
}