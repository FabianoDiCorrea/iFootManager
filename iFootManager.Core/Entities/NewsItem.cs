namespace iFootManager.Core.Entities;

public class NewsItem
{
    public string Title { get; set; }
    public string Content { get; set; }
    public NewsScope Scope { get; set; }
    public string LeagueId { get; set; } // Opcional para Global
    public DateTime Date { get; set; }

    public NewsItem(string title, string content, NewsScope scope, string leagueId = "")
    {
        Title = title;
        Content = content;
        Scope = scope;
        LeagueId = leagueId;
        Date = DateTime.Now;
    }
}
