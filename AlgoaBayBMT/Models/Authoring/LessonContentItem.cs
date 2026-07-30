using System.ComponentModel.DataAnnotations;

namespace AlgoaBayBMT.Models.Authoring;

public static class LessonContentTypes
{
    public const string Slide = "Slide";
    public const string Card = "Card";
    public const string Video = "Video";
    public const string Mcq = "MCQ";
    public const string TrueFalse = "TrueFalse";
    public const string Scenario = "Scenario";

    public static readonly IReadOnlyList<string> All =
    [
        Slide,
        Card,
        Video,
        Mcq,
        TrueFalse,
        Scenario
    ];
}

public class Lesson
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<LessonContentItem> ContentItems { get; set; } = new List<LessonContentItem>();
}

public class LessonContentItem
{
    public int Id { get; set; }
    public int LessonId { get; set; }

    [Required]
    [StringLength(32)]
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Lesson? Lesson { get; set; }
}
