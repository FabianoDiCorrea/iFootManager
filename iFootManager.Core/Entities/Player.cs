using iFootManager.Core;

namespace iFootManager.Core.Entities;

// Representa um jogador de futebol
public class Player
{
    public string Name { get; private set; }
    public Position Position { get; private set; }
    public int OverallRating { get; private set; } // Habilidade base (0-100)
    public double Energy { get; private set; } = 100; // Energia (0-100)
    public double Morale { get; private set; } = 70; // Moral (0-100)

    public Player(string name, Position position, int overallRating)
    {
        Name = name;
        Position = position;
        OverallRating = overallRating;
    }

    public void UpdateEnergy(TacticalPosture teamPosture)
    {
        // Consumo base varia por posição
        double baseConsumption = 0.5;
        if (Position == Position.Midfielder) baseConsumption = 0.8;
        if (Position == Position.Forward) baseConsumption = 0.6;

        // Fator de esforço tático
        double effortFactor = 1.0;
        switch (teamPosture)
        {
            case TacticalPosture.VeryDefensive: effortFactor = 1.2; break; // Defende muito
            case TacticalPosture.Defensive: effortFactor = 1.1; break;
            case TacticalPosture.Balanced: effortFactor = 1.0; break;
            case TacticalPosture.Offensive: effortFactor = 1.3; break;     // Corre muito
            case TacticalPosture.AllOutAttack: effortFactor = 1.5; break;  // Pressão total
        }

        // Variação aleatória do cansaço (0.8 a 1.2)
        double randomFactor = 0.8 + (new Random().NextDouble() * 0.4);

        double consumption = baseConsumption * effortFactor * randomFactor;
        
        Energy -= consumption;
        if (Energy < 0) Energy = 0;
    }
    
    public double GetEffectiveOverall()
    {
        // Formula: Base * (0.85 + Morale / 200)
        // Moral 70 -> Multiplicador 1.2
        double moraleMultiplier = 0.85 + (Morale / 200.0);
        return OverallRating * moraleMultiplier;
    }
    
    public int LowMoraleStreak { get; private set; } = 0;

    public void CheckLowMoraleStreak()
    {
        if (Morale < 30)
            LowMoraleStreak++;
        else
            LowMoraleStreak = 0;
    }

    public void UpdateMorale(double change)
    {
        Morale += change;
        if (Morale < 0) Morale = 0;
        if (Morale > 100) Morale = 100;
    }
}
