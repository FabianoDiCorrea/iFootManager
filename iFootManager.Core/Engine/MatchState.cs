using iFootManager.Core.Entities;

namespace iFootManager.Core.Engine;

// Mantém o estado atual da partida
public class MatchState
{
    public Team HomeTeam { get; }      // Time da casa
    public Team AwayTeam { get; }      // Time visitante
    public int HomeScore { get; private set; } // Placar casa
    public int AwayScore { get; private set; } // Placar visitante
    public int CurrentMinute { get; private set; } // Minuto atual
    public List<string> Events { get; } = new List<string>(); // Lista de eventos da partida

    public MatchState(Team homeTeam, Team awayTeam)
    {
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeScore = 0;
        AwayScore = 0;
        CurrentMinute = 0;
    }

    public void IncrementMinute()
    {
        CurrentMinute++;
    }

    public void AddGoal(bool isHomeTeam, string description)
    {
        if (isHomeTeam)
            HomeScore++;
        else
            AwayScore++;
        
        AddEvent(description);
    }


    public void AddEvent(string description)
    {
        Events.Add($"[{CurrentMinute}'] {description}");
    }
}
