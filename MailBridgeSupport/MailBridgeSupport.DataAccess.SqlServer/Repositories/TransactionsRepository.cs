﻿using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore.Storage;

namespace MailBridgeSupport.DataAccess.SqlServer.Repositories;

public class TransactionsRepository : ITransactionsRepository
{
    private readonly MailBridgeSupportDbContext _context;
    private IDbContextTransaction _transaction;

    public TransactionsRepository(MailBridgeSupportDbContext context)
    {
        _context = context;
    }
    
    public async Task<Result<bool>> BeginTransactionAsync()
    {
        try
        {
            _transaction = await _context.Database.BeginTransactionAsync();
            return true;
        }
        catch (Exception e)
        {
            return Result.Failure<bool>($"error message {e}");
        }
    }

    public async Task<Result<bool>> CommitAsync()
    {
        try
        {
            await _transaction.CommitAsync();
            return true;
        }
        catch (Exception e)
        {
            await _transaction.RollbackAsync();
            return Result.Failure<bool>($"error during commit {e}");
        }
    }

    public async Task<Result<bool>> RollbackAsync()
    {
        try
        {
            await _transaction.RollbackAsync();
            return true;
        }
        catch (Exception e)
        {
            return Result.Failure<bool>($"error during rollback {e}");
        }
        finally
        {
            _transaction.Dispose();
        }
    }
}