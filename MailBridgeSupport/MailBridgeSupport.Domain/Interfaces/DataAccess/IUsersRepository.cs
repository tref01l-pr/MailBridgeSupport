﻿using CSharpFunctionalExtensions;
using MailBridgeSupport.Domain.Models;

namespace MailBridgeSupport.Domain.Interfaces.DataAccess;

public interface IUsersRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User[]> GetByIdsAsync(IEnumerable<Guid> userIds);
    Task<User[]> GetAsync();
    Task<bool> Delete(Guid id);
}
