using AlgoaBayBMT.Data;
using AlgoaBayBMT.Models.Authoring;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services;

public class ContentAuthoringService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IContentAuthoringService
{
    public async Task<LessonAuthoringPageModel> GetLessonAuthoringPageAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var lesson = await dbContext.AuthoringLessons
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);

        if (lesson is null)
        {
            lesson = new Lesson
            {
                Id = lessonId,
                Title = $"Lesson {lessonId}",
                Description = "Authoring shell created for non-technical content administration.",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            dbContext.AuthoringLessons.Add(lesson);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var items = await GetContentForLessonAsync(lessonId, cancellationToken);

        return new LessonAuthoringPageModel
        {
            LessonId = lesson.Id,
            LessonTitle = lesson.Title,
            Items = items
        };
    }

    public async Task<List<LessonContentItem>> GetContentForLessonAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await dbContext.LessonContentItems
            .AsNoTracking()
            .Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<LessonContentItem> CreateContentItemAsync(LessonContentItem item, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        item.CreatedAt = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        dbContext.LessonContentItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<LessonContentItem> UpdateContentItemAsync(LessonContentItem item, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await dbContext.LessonContentItems.FirstOrDefaultAsync(x => x.Id == item.Id, cancellationToken)
            ?? throw new InvalidOperationException("Content item not found.");

        existing.ContentType = item.ContentType;
        existing.Title = item.Title;
        existing.FrontText = item.FrontText;
        existing.BodyHtml = item.BodyHtml;
        existing.BackHtml = item.BackHtml;
        existing.DescriptionHtml = item.DescriptionHtml;
        existing.VideoUrl = item.VideoUrl;
        existing.QuestionHtml = item.QuestionHtml;
        existing.ScenarioHtml = item.ScenarioHtml;
        existing.OptionA = item.OptionA;
        existing.OptionB = item.OptionB;
        existing.OptionC = item.OptionC;
        existing.OptionD = item.OptionD;
        existing.Option1 = item.Option1;
        existing.Option2 = item.Option2;
        existing.Option3 = item.Option3;
        existing.Option4 = item.Option4;
        existing.CorrectOption = item.CorrectOption;
        existing.CorrectBool = item.CorrectBool;
        existing.CorrectOptionIndex = item.CorrectOptionIndex;
        existing.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task DeleteContentItemAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var item = await dbContext.LessonContentItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null)
        {
            return;
        }

        dbContext.LessonContentItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
