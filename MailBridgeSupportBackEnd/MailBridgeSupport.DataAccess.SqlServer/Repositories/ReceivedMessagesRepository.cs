using AutoMapper;
using CSharpFunctionalExtensions;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using MailBridgeSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace MailBridgeSupport.DataAccess.SqlServer.Repositories;

public class ReceivedMessagesRepository : IReceivedMessagesRepository
{
    private readonly MailBridgeSupportDbContext _context;
    private readonly IMapper _mapper;

    public ReceivedMessagesRepository(MailBridgeSupportDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<int>> GetNumberOfMessagesAsync()
    {
        int count = await _context.ReceivedMessages.CountAsync();
        return Result.Success(count);
    }

    public async Task<Result<ReceivedMessage[]>> GetByRequesterAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<ReceivedMessage[]>("your email is empty");
        }

        var receivedMessages = await _context.ReceivedMessages
            .TagWith("Get By Requester Async messages by question email")
            .AsNoTracking()
            .Where(x => x.From == email)
            .ToArrayAsync();
        return _mapper.Map<ReceivedMessageEntity[], ReceivedMessage[]>(receivedMessages);
    }

    public async Task<Result<ReceivedMessage[]>> GetLastReceivedMessagesAsync()
    {
        var lastMessages = await _context.ReceivedMessages
            .TagWith("Get last sent messages by question email")
            .AsNoTracking()
            .GroupBy(m => m.From)
            .Select(g =>
                g.OrderByDescending(m => m.Date).FirstOrDefault()) // Выбираем последнее сообщение из каждой группы
            .ToArrayAsync();

        return _mapper.Map<ReceivedMessageEntity[], ReceivedMessage[]>(lastMessages);
    }

    public async Task<Result<int>> CreateAsync(ReceivedMessage receivedMessage)
    {
        var receivedMessageEntity = _mapper.Map<ReceivedMessage, ReceivedMessageEntity>(receivedMessage);
        await _context.ReceivedMessages.AddAsync(receivedMessageEntity);
        var result = await SaveAsync();
        if (!result.Value)
        {
            return Result.Failure<int>("Something went wrong during save message");
        }

        return receivedMessageEntity.Id;
    }

    public async Task<Result<bool>> SaveAsync() => await _context.SaveChangesAsync() > 0;
}