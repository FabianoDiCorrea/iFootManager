using iFootManager.Core.Entities;
using iFootManager.Core.Engine;

namespace iFootManager.App;

public partial class RoundPage : ContentPage
{
    private League _league;
    private Club _userClub;

    public RoundPage(League league, Club userClub)
    {
        InitializeComponent();
        _league = league;
        _userClub = userClub;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_userClub == null || _league == null) return;

        lblClubName.Text = _userClub.Name.ToUpper();
        lblDivision.Text = $"RODADA {_league.CurrentRound}";
        lblWages.Text = $"Custo mensal do elenco: {_userClub.MonthlyWageBill:C0}";

        stackFixtures.Children.Clear();
        var fixtures = _league.GetCurrentRoundFixtures();
        foreach (var match in fixtures)
        {
            var label = new Label
            {
                Text = $"{match.Home.Name} vs {match.Away.Name}",
                HorizontalOptions = LayoutOptions.Center,
                TextColor = (match.Home == _userClub || match.Away == _userClub) ? Color.FromArgb("#3B82F6") : Colors.White,
                FontAttributes = (match.Home == _userClub || match.Away == _userClub) ? FontAttributes.Bold : FontAttributes.None
            };
            stackFixtures.Children.Add(label);
        }
    }

    private async void OnAdvanceClicked(object sender, EventArgs e)
    {
        if (_league == null || _userClub == null) return;

        var fixtures = _league.GetCurrentRoundFixtures();
        var userMatch = fixtures.FirstOrDefault(m => m.Home == _userClub || m.Away == _userClub);

        if (userMatch != null)
        {
            // Simular outros jogos instantaneamente (exceto o do usuário)
            foreach (var match in fixtures.Where(m => m != userMatch))
            {
                var engine = new MatchEngine(match.Home.Squad, match.Away.Squad);
                while (engine.GetState().CurrentMinute <= 90) engine.AdvanceMinute();
                _league.ProcessMatchResult(engine.GetState(), match.Home, match.Away);
            }

            // Abrir tela para o jogo do usuário (ela cuidará do avanço ao terminar)
            await Navigation.PushAsync(new MatchPage(_league, _userClub, userMatch));
        }
        else
        {
            // Se não tem jogo do usuário nesta rodada, avança direto
            if (_league.CurrentRound % 4 == 0) _userClub.ProcessMonthlyFinancials();
            _league.AdvanceRound();
            UpdateUI();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateUI();
    }
}
