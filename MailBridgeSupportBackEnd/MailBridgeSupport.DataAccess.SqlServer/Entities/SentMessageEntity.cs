using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MailBridgeSupport.DataAccess.SqlServer.Entities;

public class SentMessageEntity
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTimeOffset Date { get; set; }
}

public class CourseEntityConfiguration : IEntityTypeConfiguration<SentMessageEntity>
{
    public void Configure(EntityTypeBuilder<SentMessageEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired(true);
        
        builder.Property(x => x.To)
            .IsRequired(true);

        builder.Property(x => x.Subject)
            .IsRequired(true);

        builder.Property(x => x.Body)
            .IsRequired(true);
        
        builder.Property(x => x.Date)
            .IsRequired(true);
    }
}