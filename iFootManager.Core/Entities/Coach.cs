namespace iFootManager.Core.Entities;

// Representa o técnico do time
public class Coach
{
    public string Name { get; set; } // Nome do técnico
    public TacticalPosture PreferredStyle { get; set; } // Estilo de jogo preferido

    public Coach(string name, TacticalPosture preferredStyle)
    {
        Name = name;
        PreferredStyle = preferredStyle;
    }
}
