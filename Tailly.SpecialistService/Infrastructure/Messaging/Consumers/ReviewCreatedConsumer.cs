using MassTransit;
using Microsoft.EntityFrameworkCore;
using Tailly.Contracts.Messages;
using Tailly.SpecialistService.Core.Entities.Reviews;
using Tailly.SpecialistService.Infrastructure.DataAccess;

namespace Tailly.SpecialistService.Infrastructure.Messaging.Consumers;

public class ReviewCreatedConsumer : IConsumer<ReviewCreatedMessage>
{
    private readonly SpecialistDbContext _context;
    private readonly ILogger<ReviewCreatedConsumer> _logger;

    public ReviewCreatedConsumer(
        SpecialistDbContext context,
        ILogger<ReviewCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReviewCreatedMessage> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received ReviewCreatedMessage: ReviewId={ReviewId}, SpecialistId={SpecialistId}",
            message.ReviewId,
            message.SpecialistId);

        var specialist = await _context.Specialists
            .Include(x => x.Reviews)
            .FirstOrDefaultAsync(x => x.Id == message.SpecialistId);

        if (specialist == null)
        {
            _logger.LogWarning(
                "Specialist not found: {SpecialistId}",
                message.SpecialistId);

            return;
        }

        var reviewExists = specialist.Reviews
            .Any(x => x.Id == message.ReviewId);

        if (reviewExists)
        {
            _logger.LogInformation(
                "Review already exists: {ReviewId}",
                message.ReviewId);

            return;
        }

        var review = new ReviewEntity
        {
            Id = message.ReviewId,
            SpecialistId = message.SpecialistId,
            OrderId = message.OrderId,

            AuthorName = message.AuthorName,
            ServiceTitle = message.ServiceTitle,
            PetName = message.PetName,

            Rating = message.Rating,
            Text = message.Text,

            CreatedAt = message.CreatedAt,
            Photos = message.Photos ?? []
        };

        await _context.Reviews.AddAsync(review);

        specialist.ReviewsCount += 1;

        var ratings = await _context.Reviews
            .Where(x => x.SpecialistId == specialist.Id)
            .Select(x => x.Rating)
            .ToListAsync();

        ratings.Add(review.Rating);

        specialist.Rating = decimal.Round(
            (decimal)ratings.Average(),
            1);

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Review successfully added. ReviewId={ReviewId}",
            message.ReviewId);
    }
}