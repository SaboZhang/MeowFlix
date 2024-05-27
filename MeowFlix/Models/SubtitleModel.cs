using MeowFlix.Database.Tables;

namespace MeowFlix.Models;
public class SubtitleModel : BaseMediaTable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Translator { get; set; }
    public string Language { get; set; }
    public bool IsActive { get; set; }

    public SubtitleModel(string title, string filePath, string dateTime, double fileSize, ServerType serverType, string channel, int? year)
        : base(title, filePath, dateTime, fileSize, serverType, channel, year)
    {
    }

    public SubtitleModel(string title, string filePath)
    {
        this.Title = title;
        this.FilePath = filePath;
        this.ServerType = ServerType.Subtitle;
    }

    public SubtitleModel(string name, string title, string filePath, string description, string language, string translator)
    {
        this.Name = name;
        this.Title = title;
        this.FilePath = filePath;
        this.Description = description;
        this.Language = language;
        this.Translator = translator;
        this.ServerType = ServerType.Subtitle;
    }
}
