using Microsoft.EntityFrameworkCore;
using Tailly.PostsService.Core.Entities;
using Tailly.PostsService.Infrastructure.Configurations.Entities;

namespace Tailly.PostsService.Infrastructure.DataAccess;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new PostImageConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new PostTagConfiguration());
        modelBuilder.ApplyConfiguration(new BannerConfiguration());
    }

    public DbSet<PostEntity> Posts { get; set; }
    public DbSet<PostImageEntity> PostImages { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<PostTagEntity> PostTags { get; set; }
    public DbSet<BannerEntity> Banners { get; set; }
}