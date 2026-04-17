using AlgoaBayBMT.Models.Authoring;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Notifications;

namespace AlgoaBayBMT.Components.Pages.Authoring;

public class LessonAuthoringBase : ComponentBase
{
    [Inject]
    protected IContentAuthoringService ContentAuthoringService { get; set; } = default!;

    [Parameter]
    public int LessonId { get; set; }

    protected SfToast SuccessToast = default!;
    protected LessonAuthoringPageModel? PageModel;
    protected LessonAuthoringItemModel EditorModel { get; set; } = new();
    protected string SelectedContentType { get; set; } = LessonContentTypes.Slide;
    protected string? StatusMessage { get; set; }

    protected IReadOnlyList<ContentTypeOption> ContentTypeOptions =>
    [
        new() { Text = "Slide", Value = LessonContentTypes.Slide },
        new() { Text = "Card", Value = LessonContentTypes.Card },
        new() { Text = "Video", Value = LessonContentTypes.Video },
        new() { Text = "MCQ", Value = LessonContentTypes.Mcq },
        new() { Text = "True / False", Value = LessonContentTypes.TrueFalse },
        new() { Text = "Scenario", Value = LessonContentTypes.Scenario }
    ];

    protected IReadOnlyList<string> McqAnswerOptions => ["A", "B", "C", "D"];

    protected IReadOnlyList<BooleanOption> BooleanOptions =>
    [
        new("True", true),
        new("False", false)
    ];

    protected IReadOnlyList<IndexedOption> ScenarioAnswerOptions =>
    [
        new("Option 1", 1),
        new("Option 2", 2),
        new("Option 3", 3),
        new("Option 4", 4)
    ];

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
        ResetEditor();
    }

    protected async Task OnContentTypeChanged(ChangeEventArgs<string, ContentTypeOption> args)
    {
        SelectedContentType = args.Value ?? LessonContentTypes.Slide;
        ResetEditor();
        await InvokeAsync(StateHasChanged);
    }

    protected async Task SaveAsync()
    {
        var validationMessage = ValidateEditor();
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            StatusMessage = validationMessage;
            return;
        }

        var entity = MapToEntity(EditorModel);

        if (entity.Id == 0)
        {
            await ContentAuthoringService.CreateContentItemAsync(entity);
            await ShowSuccessToastAsync("Content item created.");
        }
        else
        {
            await ContentAuthoringService.UpdateContentItemAsync(entity);
            await ShowSuccessToastAsync("Content item updated.");
        }

        await LoadAsync();
        ResetEditor();
    }

    protected void ResetEditor()
    {
        EditorModel = new LessonAuthoringItemModel
        {
            LessonId = LessonId,
            ContentType = SelectedContentType
        };
        StatusMessage = null;
    }

    protected void EditItem(LessonContentItem item)
    {
        SelectedContentType = item.ContentType;
        EditorModel = new LessonAuthoringItemModel
        {
            Id = item.Id,
            LessonId = item.LessonId,
            ContentType = item.ContentType,
            Title = item.Title,
            FrontText = item.FrontText,
            BodyHtml = item.BodyHtml,
            BackHtml = item.BackHtml,
            DescriptionHtml = item.DescriptionHtml,
            VideoUrl = item.VideoUrl,
            QuestionHtml = item.QuestionHtml,
            ScenarioHtml = item.ScenarioHtml,
            OptionA = item.OptionA,
            OptionB = item.OptionB,
            OptionC = item.OptionC,
            OptionD = item.OptionD,
            Option1 = item.Option1,
            Option2 = item.Option2,
            Option3 = item.Option3,
            Option4 = item.Option4,
            CorrectOption = item.CorrectOption,
            CorrectBool = item.CorrectBool,
            CorrectOptionIndex = item.CorrectOptionIndex
        };
        StatusMessage = null;
    }

    protected async Task DeleteItemAsync(int id)
    {
        await ContentAuthoringService.DeleteContentItemAsync(id);
        await LoadAsync();
        await ShowSuccessToastAsync("Content item deleted.");
    }

    protected string GetItemTitle(LessonContentItem item)
        => item.ContentType switch
        {
            LessonContentTypes.Slide => item.Title ?? "Untitled slide",
            LessonContentTypes.Card => item.FrontText ?? "Untitled card",
            LessonContentTypes.Video => item.Title ?? "Untitled video",
            LessonContentTypes.Mcq => "Multiple Choice Question",
            LessonContentTypes.TrueFalse => "True / False Question",
            LessonContentTypes.Scenario => "Scenario Question",
            _ => item.ContentType
        };

    protected string GetItemPreview(LessonContentItem item)
        => item.ContentType switch
        {
            LessonContentTypes.Slide => StripHtml(item.BodyHtml),
            LessonContentTypes.Card => StripHtml(item.BackHtml),
            LessonContentTypes.Video => StripHtml(item.DescriptionHtml),
            LessonContentTypes.Mcq => StripHtml(item.QuestionHtml),
            LessonContentTypes.TrueFalse => StripHtml(item.QuestionHtml),
            LessonContentTypes.Scenario => StripHtml(item.ScenarioHtml),
            _ => string.Empty
        };

    private async Task LoadAsync()
    {
        PageModel = await ContentAuthoringService.GetLessonAuthoringPageAsync(LessonId);
        StatusMessage = null;
    }

    private LessonContentItem MapToEntity(LessonAuthoringItemModel model) => new()
    {
        Id = model.Id ?? 0,
        LessonId = LessonId,
        ContentType = model.ContentType,
        Title = Clean(model.Title),
        FrontText = Clean(model.FrontText),
        BodyHtml = CleanHtml(model.BodyHtml),
        BackHtml = CleanHtml(model.BackHtml),
        DescriptionHtml = CleanHtml(model.DescriptionHtml),
        VideoUrl = Clean(model.VideoUrl),
        QuestionHtml = CleanHtml(model.QuestionHtml),
        ScenarioHtml = CleanHtml(model.ScenarioHtml),
        OptionA = Clean(model.OptionA),
        OptionB = Clean(model.OptionB),
        OptionC = Clean(model.OptionC),
        OptionD = Clean(model.OptionD),
        Option1 = Clean(model.Option1),
        Option2 = Clean(model.Option2),
        Option3 = Clean(model.Option3),
        Option4 = Clean(model.Option4),
        CorrectOption = Clean(model.CorrectOption),
        CorrectBool = model.CorrectBool,
        CorrectOptionIndex = model.CorrectOptionIndex
    };

    private string? ValidateEditor()
        => SelectedContentType switch
        {
            LessonContentTypes.Slide when string.IsNullOrWhiteSpace(EditorModel.Title) || string.IsNullOrWhiteSpace(StripHtml(EditorModel.BodyHtml))
                => "Slide title and body are required.",
            LessonContentTypes.Card when string.IsNullOrWhiteSpace(EditorModel.FrontText) || string.IsNullOrWhiteSpace(StripHtml(EditorModel.BackHtml))
                => "Flashcard front text and back text are required.",
            LessonContentTypes.Video when string.IsNullOrWhiteSpace(EditorModel.Title) || string.IsNullOrWhiteSpace(StripHtml(EditorModel.DescriptionHtml)) || string.IsNullOrWhiteSpace(EditorModel.VideoUrl)
                => "Video title, description, and URL are required.",
            LessonContentTypes.Mcq when string.IsNullOrWhiteSpace(StripHtml(EditorModel.QuestionHtml)) || string.IsNullOrWhiteSpace(EditorModel.OptionA) || string.IsNullOrWhiteSpace(EditorModel.OptionB) || string.IsNullOrWhiteSpace(EditorModel.OptionC) || string.IsNullOrWhiteSpace(EditorModel.OptionD) || string.IsNullOrWhiteSpace(EditorModel.CorrectOption)
                => "Complete the question, all four options, and select the correct answer.",
            LessonContentTypes.TrueFalse when string.IsNullOrWhiteSpace(StripHtml(EditorModel.QuestionHtml)) || EditorModel.CorrectBool is null
                => "Enter the true/false question and select the correct answer.",
            LessonContentTypes.Scenario when string.IsNullOrWhiteSpace(StripHtml(EditorModel.ScenarioHtml)) || string.IsNullOrWhiteSpace(EditorModel.Option1) || string.IsNullOrWhiteSpace(EditorModel.Option2) || string.IsNullOrWhiteSpace(EditorModel.Option3) || string.IsNullOrWhiteSpace(EditorModel.Option4) || EditorModel.CorrectOptionIndex is null
                => "Complete the scenario, all four options, and select the correct answer.",
            _ => null
        };

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? CleanHtml(string? value)
    {
        var cleaned = Clean(value);
        return string.IsNullOrWhiteSpace(cleaned) ? null : cleaned;
    }

    private static string StripHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty).Trim();
    }

    private async Task ShowSuccessToastAsync(string message)
    {
        await SuccessToast.ShowAsync(new ToastModel
        {
            Title = "Success",
            Content = message,
            CssClass = "e-toast-success",
            ShowCloseButton = true,
            Timeout = 3500
        });
    }

    protected sealed record BooleanOption(string Text, bool Value);
    protected sealed record IndexedOption(string Text, int Value);
}
