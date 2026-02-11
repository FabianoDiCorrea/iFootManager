namespace iFootManager.Core.Entities;

// Representa um time de futebol
public class Team
{
    public string Name { get; set; }
    public Coach Coach { get; set; }
    public List<Player> Players { get; private set; } = new List<Player>();
    public List<Player> StartingEleven { get; private set; } = new List<Player>(); // Titulares
    public List<Player> Bench { get; private set; } = new List<Player>();          // Banco de reservas
    public TacticalPosture CurrentTacticalPosture { get; private set; }

    public Team(string name, Coach coach)
    {
        Name = name;
        Coach = coach;
        CurrentTacticalPosture = coach.PreferredStyle; // Define a tática inicial conforme a preferência do técnico
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void SetStartingEleven(List<Player> starters)
    {
        if (starters.Count != 11)
            throw new ArgumentException("O time titular deve ter exatamente 11 jogadores.");

        StartingEleven = new List<Player>(starters);
        
        // Jogadores que não são titulares vão para o banco
        Bench = Players.Except(StartingEleven).ToList();
    }

    public void ChangeTacticalPosture(TacticalPosture newPosture)
    {
        CurrentTacticalPosture = newPosture;
    }

    public bool ApplySubstitution(Player playerOut, Player playerIn)
    {
        if (!StartingEleven.Contains(playerOut))
            return false; // Jogador que sai não está jogando

        if (!Bench.Contains(playerIn))
            return false; // Jogador que entra não está no banco

        StartingEleven.Remove(playerOut);
        StartingEleven.Add(playerIn);

        Bench.Remove(playerIn);
        Bench.Add(playerOut); // Jogador que sai vai para o banco
        
        return true;
    }
}
