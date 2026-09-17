using Tailly.SpecialistService.Infrastructure.Repositories.Interfaces;

namespace Tailly.SpecialistService.Application.Helpers;

public static class SpecialistSlugGenerator
{
    public static async Task<string> GenerateUniqueSlugAsync(string firstName, string lastName, ISpecialistRepository specialistRepository)
    {
        string baseSlug = $"{firstName.ToLower().Trim()}-{lastName.ToLower().Trim()}"
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace(".", "");

        string slug = baseSlug;
        int counter = 1;

        while (await specialistRepository.SlugExistsAsync(slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}