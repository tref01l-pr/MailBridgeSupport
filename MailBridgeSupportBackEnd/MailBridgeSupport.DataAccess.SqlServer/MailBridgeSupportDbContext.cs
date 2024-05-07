using MailBridgeSupport.DataAccess.SqlServer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MailBridgeSupport.DataAccess.SqlServer;

public class MailBridgeSupportDbContext : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
{
    public MailBridgeSupportDbContext(DbContextOptions<MailBridgeSupportDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<SentMessageEntity> SentMessages { get; set; }
    public DbSet<SessionEntity> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MailBridgeSupportDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}