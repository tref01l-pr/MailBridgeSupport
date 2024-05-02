using MailBridgeSupport.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailBridgeSupport.DataAccess.SqlServer.Entities;

public class SessionEntity
{
    public Guid UserId { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
}

public class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
{
    public void Configure(EntityTypeBuilder<SessionEntity> builder)
    {
        builder.HasKey(x => x.UserId);

        builder.Property(x => x.AccessToken)
            .HasMaxLength(Session.MaxLengthToken)
            .IsRequired(true);

        builder.Property(x => x.RefreshToken)
            .HasMaxLength(Session.MaxLengthToken)
            .IsRequired(true);
    }
}