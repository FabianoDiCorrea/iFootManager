using iFootManager.Core.Engine;

namespace iFootManager.Core.Entities;

public class Matchup
{
    public Club Home { get; set; }
    public Club Away { get; set; }

    public Matchup(Club home, Club away)
    {
        Home = home;
        Away = away;
    }
}

public class League
{
    public string Name { get; private set; }
    public List<Club> Clubs { get; private set; }
    public List<LeagueTableEntry> Table { get; private set; }
    public List<List<Matchup>> Schedule { get; private set; } // Lista de Rodadas
    public int CurrentRound { get; private set; } = 1;

    public int TotalRounds => (Clubs.Count - 1) * 2; // Turno e Returno

    public League(string name, List<Club> clubs)
    {
        Name = name;
        Clubs = clubs;
        Table = new List<LeagueTableEntry>();
        foreach (var club in clubs)
        {
            Table.Add(new LeagueTableEntry(club));
        }
        
        GenerateSchedule();
    }

    private void GenerateSchedule()
    {
        Schedule = new List<List<Matchup>>();
        int numClubs = Clubs.Count;
        int numRounds = numClubs - 1;
        int halfSize = numClubs / 2;

        List<Club> tempClubs = new List<Club>(Clubs);
        // Se ímpar, precisaria de um "BYE", mas assumimos par (4 clubes) por enquanto.

        // Algoritmo Round Robin para o Turno
        for (int round = 0; round < numRounds; round++)
        {
            List<Matchup> fixtures = new List<Matchup>();
            for (int i = 0; i < halfSize; i++)
            {
                int homeIdx = (round + i) % (numClubs - 1);
                int awayIdx = (numClubs - 1 - i + round) % (numClubs - 1);
                
                // O último time fica fixo no índice numClubs-1, alternando home/away
                if (i == 0) awayIdx = numClubs - 1;

                Club home = tempClubs[homeIdx];
                Club away = tempClubs[awayIdx];

                // Alternar mando de campo a cada rodada para o time fixo
                if (i == 0 && round % 2 == 1)
                {
                    (home, away) = (away, home);
                }
                
                fixtures.Add(new Matchup(home, away));
            }
            Schedule.Add(fixtures);
        }

        // Duplicar para o Returno (Invertendo mando)
        int initialRounds = Schedule.Count;
        for (int r = 0; r < initialRounds; r++)
        {
            List<Matchup> returnFixtures = new List<Matchup>();
            foreach (var match in Schedule[r])
            {
                returnFixtures.Add(new Matchup(match.Away, match.Home));
            }
            Schedule.Add(returnFixtures);
        }
    }

    public List<Matchup> GetCurrentRoundFixtures()
    {
        if (CurrentRound > Schedule.Count) return new List<Matchup>();
        return Schedule[CurrentRound - 1];
    }

    public void ProcessMatchResult(MatchState result, Club home, Club away)
    {
        // Atualizar Tabela
        var homeEntry = Table.First(e => e.Club == home);
        var awayEntry = Table.First(e => e.Club == away);

        homeEntry.Update(result.HomeScore, result.AwayScore);
        awayEntry.Update(result.AwayScore, result.HomeScore);
        
        // Avaliação de Carreira (Só se for o time do usuário, mas podemos rodar para todos se quisermos simular IA)
        // Por simplificação, vamos assumir que o Program.cs chama EvaluateMatch manualmente para o usuário
    }

    public void AdvanceRound()
    {
        CurrentRound++;
    }

    public List<LeagueTableEntry> GetStandings()
    {
        // Ordenar por Pontos DESC, Vitórias DESC, Saldo de Gols DESC, Gols Pró DESC
        return Table.OrderByDescending(e => e.Points)
                    .ThenByDescending(e => e.Won)
                    .ThenByDescending(e => e.GoalDifference)
                    .ThenByDescending(e => e.GoalsFor)
                    .ToList();
    }
}
