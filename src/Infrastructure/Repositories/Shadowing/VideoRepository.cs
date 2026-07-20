using Fluentra.Domain.Entities.Shadowing;
using Fluentra.Domain.Interfaces.Shadowing;
using Fluentra.Domain.ValueObjects.Shadowing;
using Fluentra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fluentra.Infrastructure.Repositories.Shadowing;

public sealed class VideoRepository : IVideoRepository
{
    private readonly AppDbContext _context;

    public VideoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddAsync(Video video)
    {
        var result = await _context.Set<Video>().AddAsync(video);
        return result.State == EntityState.Added;
    }

    public async Task<Video?> GetByYouTubeVideoIdAsync(string youTubeVideoId)
    {
        var value = new YouTubeVideoId(youTubeVideoId);

        return await _context.Set<Video>()
            .Include(x => x.Scenes)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.YouTubeVideoId == value);
    }
}
