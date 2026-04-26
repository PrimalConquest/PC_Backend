using DBWrapper.Source.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DBWrapper.Source.Configs
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).UseIdentityColumn();
            builder.Property(t => t.TokenHash).IsRequired();
            builder.Property(t => t.ExpiresAt).IsRequired();
            builder.Property(t => t.CreatedAt).IsRequired();

            // Fast lookup by hash on every refresh/logout
            builder.HasIndex(t => t.TokenHash).IsUnique();
            // Fast sweep by user on every login/logout
            builder.HasIndex(t => t.UserId);

            builder.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
