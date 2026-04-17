using AlgoaBayBMT.Models.Authoring;
using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Services.Models;

public sealed class LessonAuthoringItemModel
{
    public int? Id { get; set; }
    public int LessonId { get; set; }
    public string ContentType { get; set; } = LessonContentTypes.Slide;

    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? FrontText { get; set; }

    public string? BodyHtml { get; set; }
    public string? BackHtml { get; set; }
    public string? DescriptionHtml { get; set; }

    [StringLength(1000)]
    public string? VideoUrl { get; set; }

    public string? QuestionHtml { get; set; }
    public string? ScenarioHtml { get; set; }

    [StringLength(500)]
    public string? OptionA { get; set; }

    [StringLength(500)]
    public string? OptionB { get; set; }

    [StringLength(500)]
    public string? OptionC { get; set; }

    [StringLength(500)]
    public string? OptionD { get; set; }

    [StringLength(500)]
    public string? Option1 { get; set; }

    [StringLength(500)]
    public string? Option2 { get; set; }

    [StringLength(500)]
    public string? Option3 { get; set; }

    [StringLength(500)]
    public string? Option4 { get; set; }

    [StringLength(50)]
    public string? CorrectOption { get; set; }

    public bool? CorrectBool { get; set; }
    public int? CorrectOptionIndex { get; set; }
}

public sealed class LessonAuthoringPageModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public List<LessonContentItem> Items { get; set; } = new();
}

public sealed class ContentTypeOption
{
    public string Text { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
