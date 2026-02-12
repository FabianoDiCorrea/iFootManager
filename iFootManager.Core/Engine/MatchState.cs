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

    public int LastHomeGoalMinute { get; private set; } = -99;
    public int LastAwayGoalMinute { get; private set; } = -99;
    
    public int HomeChances { get; private set; }
    public int AwayChances { get; private set; }
    
    public Dictionary<Player, int> GoalScorers { get; private set; }

    public MatchState(Team homeTeam, Team awayTeam)
    {
        HomeTeam = homeTeam;
        AwayTeam = awayTeam;
        HomeScore = 0;
        AwayScore = 0;
        CurrentMinute = 0;
        HomeChances = 0;
        AwayChances = 0;
        GoalScorers = new Dictionary<Player, int>();
    }

    public void IncrementMinute()
    {
        CurrentMinute++;
    }

    public void AddGoal(bool isHomeTeam, Player scorer, string description)
    {
        if (isHomeTeam)
        {
            HomeScore++;
            HomeChances++; // Gol conta como chance
            LastHomeGoalMinute = CurrentMinute;
        }
        else
        {
            AwayScore++;
            AwayChances++; // Gol conta como chance
            LastAwayGoalMinute = CurrentMinute;
        }
        
        if (scorer != null)
        {
            if (!GoalScorers.ContainsKey(scorer)) GoalScorers[scorer] = 0;
            GoalScorers[scorer]++;
        }
        
        AddEvent(description);
    }
    
    public void AddChance(bool isHomeTeam, string description)
    {
        if (isHomeTeam) HomeChances++;
        else AwayChances++;
        
        AddEvent(description);
    }


    public void AddEvent(string description)
    {
        Events.Add($"[{CurrentMinute}'] {description}");
    }
}
