﻿using AutoMapper;
using MailBridgeSupport.API;
using MailBridgeSupport.DataAccess.SqlServer;
using MailBridgeSupport.DataAccess.SqlServer.Entities;
using MailBridgeSupport.Domain.Interfaces;
using MailBridgeSupport.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public class Seed
{
    private readonly MailBridgeSupportDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<UserEntity> _userManager;
    private readonly IImapService _imapService;
    private readonly ImapOptions _imapOptions;
    private readonly IMapper _mapper;

    public Seed(MailBridgeSupportDbContext context,
        RoleManager<IdentityRole<Guid>> roleManager,
        UserManager<UserEntity> userManager,
        IImapService imapService,
        IOptions<ImapOptions> imapOptions,
        IMapper mapper)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _imapService = imapService;
        _imapOptions = imapOptions.Value;
        _mapper = mapper;
    }

    public async Task SeedDataContextAsync()
    {
        try
        {
            if (_context.Users.FirstOrDefault(u => u.UserName == "systemadmin") is null)
            {
                var mailSystemAdmin = new UserEntity
                {
                    UserName = "systemadmin",
                    Email = "systemadmin@systemadmin.systemadmin",
                };

                var result = await _userManager.CreateAsync(mailSystemAdmin, "string");

                if (!result.Succeeded)
                {
                    return;
                }

                var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.SystemAdmin));
                if (!roleExists)
                {
                    var role = new IdentityRole<Guid>()
                    {
                        Name = nameof(Roles.SystemAdmin)
                    };

                    await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(mailSystemAdmin, nameof(Roles.SystemAdmin));
            }

            if (_context.Users.FirstOrDefault(u => u.UserName == "user") is null)
            {
                var mailSystemAdmin = new UserEntity
                {
                    UserName = "user",
                    Email = "user@user.user",
                };

                var result = await _userManager.CreateAsync(mailSystemAdmin, "string");

                if (!result.Succeeded)
                {
                    return;
                }

                var roleExists = await _roleManager.RoleExistsAsync(nameof(Roles.User));
                if (!roleExists)
                {
                    var role = new IdentityRole<Guid>()
                    {
                        Name = nameof(Roles.User)
                    };

                    await _roleManager.CreateAsync(role);
                }

                await _userManager.AddToRoleAsync(mailSystemAdmin, nameof(Roles.User));
            }

            if (!_context.ReceivedMessages.Any())
            {
                var newMessages = await _imapService.GetNewMessages(_imapOptions, 0);

                if (newMessages.IsFailure)
                {
                    throw new Exception(newMessages.Error);
                }

                var newMessagesEntity =
                    _mapper.Map<ReceivedMessage[], ReceivedMessageEntity[]>(newMessages.Value.ToArray());

                foreach (var newMessage in newMessagesEntity)
                {
                    await _context.ReceivedMessages.AddAsync(newMessage);
                }
            }

            if (!_context.SentMessages.Any())
            {
                var sentMessages = await _imapService.GetAllSentMessages(_imapOptions);

                if (sentMessages.IsFailure)
                {
                    throw new Exception(sentMessages.Error);
                }

                var sentMessagesEntity = _mapper.Map<SentMessage[], SentMessageEntity[]>(sentMessages.Value.ToArray());

                foreach (var sentMessage in sentMessagesEntity)
                {
                    await _context.SentMessages.AddAsync(sentMessage);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}