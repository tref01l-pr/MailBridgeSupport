namespace MailBridgeSupport.DataAccess.SqlServer.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReceivedMessageEntity
{
    public int Id { get; set; }

    public int MsgId { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public string Subject { get; set; }

    public string Body { get; set; }

    public DateTimeOffset Date { get; set; }
}

public class RecievedMessageEntityConfiguration : IEntityTypeConfiguration<ReceivedMessageEntity>
{
    public void Configure(EntityTypeBuilder<ReceivedMessageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MsgId)
            .IsRequired(true);

        builder.Property(x => x.From)
            .IsRequired(true);

        builder.Property(x => x.To)
            .IsRequired(true);

        builder.Property(x => x.Subject)
            .IsRequired(true);

        builder.Property(x => x.Body)
            .IsRequired(true);

        builder.Property(x => x.Date)
            .IsRequired(true);

        builder.HasIndex(msg => msg.MsgId)
            .IsUnique();
    }
}