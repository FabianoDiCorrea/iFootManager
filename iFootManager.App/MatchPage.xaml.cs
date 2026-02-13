using System.Collections.ObjectModel;
using iFootManager.Core.Entities;
using iFootManager.Core.Engine;

namespace iFootManager.App;

public partial class MatchPage : ContentPage
{
    private League _league;
    private Club _userClub;
    private Matchup _currentMatch;
    private MatchEngine _engine;
    private IDispatcherTimer _timer;
    private int _displayMinute = 0;
    private List<string> _eventsProcessed = new();

    public MatchPage(League league, Club userClub, Matchup match)
    {
        InitializeComponent();
        _league = league;
        _userClub = userClub;
        _currentMatch = match;

        lblCompetition.Text = "PREMIER LEAGUE";
        lblRound.Text = $"RODADA {_league.CurrentRound}";
        lblHomeTeam.Text = match.Home.Name.ToUpper();
        lblAwayTeam.Text = match.Away.Name.ToUpper();

        _engine = new MatchEngine(match.Home.Squad, match.Away.Squad);
        
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(50); // Simulação rápida
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_displayMinute >= 90)
        {
            _timer.Stop();
            FinishMatch();
            return;
        }

        _displayMinute++;
        lblMinute.Text = _displayMinute.ToString();
        
        // Atualizar barra (baseado na largura da tela - ratio)
        double ratio = _displayMinute / 90.0;
        barMatchTime.WidthRequest = Math.Max(1, (this.Width - 50) * ratio);

        // Avançar engine
        _engine.AdvanceMinute();
        var state = _engine.GetState();

        // Atualizar Placar
        lblHomeScore.Text = state.HomeScore.ToString();
        lblAwayScore.Text = state.AwayScore.ToString();

        // Adicionar eventos novos ao log
        foreach (var evt in state.Events)
        {
            if (!_eventsProcessed.Contains(evt))
            {
                _eventsProcessed.Add(evt);
                AddEventToUI(evt);
            }
        }
    }

    private void AddEventToUI(string message)
    {
        var label = new Label
        {
            Text = message,
            TextColor = message.Contains("GOL") ? Color.FromArgb("#FACC15") : Color.FromArgb("#94A3B8"),
            FontSize = 14,
            FontAttributes = message.Contains("GOL") ? FontAttributes.Bold : FontAttributes.None
        };
        stackEvents.Children.Insert(0, label); // Mais recente no topo
    }

    private void FinishMatch()
    {
        var finalState = _engine.GetState();
        _league.ProcessMatchResult(finalState, _currentMatch.Home, _currentMatch.Away);

        if (_currentMatch.Home == _userClub) _userClub.EvaluateMatch(finalState, true, false);
        if (_currentMatch.Away == _userClub) _userClub.EvaluateMatch(finalState, false, false);

        // Avançar rodada após o jogo do usuário
        if (_league.CurrentRound % 4 == 0) _userClub.ProcessMonthlyFinancials();
        _league.AdvanceRound();

        btnContinue.IsVisible = true;
    }

    private async void OnContinueClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
