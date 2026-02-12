using iFootManager.Core.Entities;

namespace iFootManager.Core.Entities;

public class LeagueTableEntry
{
    public Club Club { get; private set; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int Points { get; private set; }
    
    public int GoalDifference => GoalsFor - GoalsAgainst;

    public LeagueTableEntry(Club club)
    {
        Club = club;
    }

    public void Update(int goalsFor, int goalsAgainst)
    {
        Played++;
        GoalsFor += goalsFor;
        GoalsAgainst += goalsAgainst;

        if (goalsFor > goalsAgainst)
        {
            Won++;
            Points += 3;
        }
        else if (goalsFor == goalsAgainst)
        {
            Drawn++;
            Points += 1;
        }
        else
        {
            Lost++;
        }
    }
}
