using iFootManager.Core.Entities;
using iFootManager.Core.Engine;
using iFootManager.Core;

namespace iFootManager.App;

public partial class CoachHubPage : ContentPage
{
    private League _league;
    private Club _userClub;
    private List<NewsItem> _allNews = new();
    private NewsScope _currentScope = NewsScope.Global;

    public CoachHubPage()
    {
        InitializeComponent();
        
        // Inicialização de teste
        var clubs = new List<Club>
        {
            new Club("Man City", ClubSize.Large, "Premier League", "Título"),
            new Club("Liverpool", ClubSize.Large, "Premier League", "Vaga UCL"),
            new Club("Arsenal", ClubSize.Large, "Premier League", "Vaga UCL"),
            new Club("Chelsea", ClubSize.Large, "Premier League", "Vaga UEL")
        };

        _league = new League("Premier League", clubs);
        _userClub = clubs.First();
        
        InitializeLeagueSquads();
        GenerateMockNews();
        UpdateUI();
    }

    private void GenerateMockNews()
    {
        _allNews = new List<NewsItem>
        {
            new NewsItem("MERCADO AQUECIDO", "Grandes clubes Europeus buscam reforços no Brasil.", NewsScope.Global),
            new NewsItem("NOVO RECORDE", "Lionel Messi atinge marca histórica de gols na carreira.", NewsScope.Global),
            new NewsItem("FIFA ANUNCIA MUDANÇAS", "Novas regras de impedimento serão testadas em 2026.", NewsScope.Global),
            
            new NewsItem("DISPUTA PELO TOPO", $"A {_league.Name} promete ser uma das mais equilibradas.", NewsScope.League, _league.Name),
            new NewsItem("CRISE NO RIVAL", "Adversário direto enfrenta problemas financeiros.", NewsScope.League, _league.Name),
            new NewsItem("RODADA DECISIVA", "Jogos deste fim de semana podem mudar a liderança.", NewsScope.League, _league.Name)
        };
    }

    private void InitializeLeagueSquads()
    {
        foreach (var club in _league.Clubs)
        {
            if (club.Squad != null) continue;

            var coach = new Coach($"{club.Name} Coach", TacticalPosture.Balanced, CoachSpecialty.Motivator);
            var team = new Team(club.Name, coach);
            
            // Criar 11 jogadores genéricos com rating próximo ao "tamanho" do clube
            int baseRating = club.Size == ClubSize.Large ? 80 : 70;
            
            // Goleiro
            team.AddPlayer(new Player("GK", Position.Goalkeeper, baseRating + 5));
            // Outros 10
            for (int i = 0; i < 10; i++)
            {
                var pos = i < 4 ? Position.Defender : (i < 8 ? Position.Midfielder : Position.Forward);
                team.AddPlayer(new Player($"{club.Name} P{i}", pos, baseRating + new Random().Next(-5, 5)));
            }

            team.SetStartingEleven(team.Players.Take(11).ToList());
            club.Squad = team;
        }
    }

    private void UpdateUI()
    {
        lblClubName.Text = _userClub.Name.ToUpper();
        lblBalance.Text = _userClub.Balance.ToString("C0");
        lblRound.Text = $"RODADA {_league.CurrentRound}";
        lblWageSummary.Text = $"{_userClub.MonthlyWageBill:C0}/mês";
        btnLeagueNews.Text = $"iFoot News – {_league.Name.ToUpper()}";
        
        // Mock de adversário (baseado na rodada atual)
        var fixtures = _league.GetCurrentRoundFixtures();
        var userMatch = fixtures.FirstOrDefault(m => m.Home == _userClub || m.Away == _userClub);
        if (userMatch != null)
        {
            var adversary = userMatch.Home == _userClub ? userMatch.Away : userMatch.Home;
            lblNextAdversary.Text = $"VS {adversary.Name.ToUpper()}";
        }

        // Pressão do técnico
        double pressure = Math.Clamp(_userClub.CoachPressure / 100.0, 0.01, 1.0);
        barCoachPressure.WidthRequest = 300 * pressure;

        UpdateNewsFeed();
    }

    private void UpdateNewsFeed()
    {
        stackNewsFeed.Children.Clear();
        
        var filteredNews = _allNews.Where(n => 
            n.Scope == _currentScope && 
            (_currentScope == NewsScope.Global || n.LeagueId == _league.Name)
        ).ToList();

        foreach (var news in filteredNews)
        {
            var newsContainer = new VerticalStackLayout { Spacing = 5 };
            
            var titleLabel = new Label 
            { 
                Text = news.Title.ToUpper(), 
                FontSize = 14, 
                FontAttributes = FontAttributes.Bold, 
                TextColor = _currentScope == NewsScope.Global ? Color.FromArgb("#3B82F6") : Colors.White 
            };
            
            var contentLabel = new Label 
            { 
                Text = news.Content, 
                FontSize = 12, 
                TextColor = Color.FromArgb("#94A3B8") 
            };

            var separator = new BoxView { HeightRequest = 1, Color = Color.FromArgb("#1E293B"), Margin = new Thickness(0, 5) };

            newsContainer.Children.Add(titleLabel);
            newsContainer.Children.Add(contentLabel);
            newsContainer.Children.Add(separator);

            stackNewsFeed.Children.Add(newsContainer);
        }
    }

    private void OnNewsTabClicked(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        if (btn == btnWorldNews)
        {
            _currentScope = NewsScope.Global;
            btnWorldNews.BackgroundColor = Color.FromArgb("#3B82F6");
            btnWorldNews.TextColor = Colors.White;
            btnLeagueNews.BackgroundColor = Colors.Transparent;
            btnLeagueNews.TextColor = Color.FromArgb("#94A3B8");
        }
        else
        {
            _currentScope = NewsScope.League;
            btnLeagueNews.BackgroundColor = Color.FromArgb("#3B82F6");
            btnLeagueNews.TextColor = Colors.White;
            btnWorldNews.BackgroundColor = Colors.Transparent;
            btnWorldNews.TextColor = Color.FromArgb("#94A3B8");
        }

        UpdateNewsFeed();
    }

    private async void OnStartRoundClicked(object sender, EventArgs e)
    {
        // Navega para a página de rodada
        await Navigation.PushAsync(new RoundPage(_league, _userClub));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUI();
    }
}
