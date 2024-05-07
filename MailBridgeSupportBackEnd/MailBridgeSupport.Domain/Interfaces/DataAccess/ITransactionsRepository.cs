using CSharpFunctionalExtensions;

namespace MailBridgeSupport.Domain.Interfaces.DataAccess;

public interface ITransactionsRepository
{
    Task<Result<bool>> BeginTransactionAsync();

    Task<Result<bool>> CommitAsync();

    Task<Result<bool>> RollbackAsync();
}