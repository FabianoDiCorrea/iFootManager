using iFootManager.Core.Entities;

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
    public double MatchStrength { get; private set; } // Força final (com postura)
    public double BaseStrength { get; private set; }  // Soma dos overalls
    
    // Força Final dos Setores (Com Energia)
    public double DefenseStrength { get; private set; }
    public double MidfieldStrength { get; private set; }
    public double AttackStrength { get; private set; }

    // Força Base dos Setores (Sem Energia - Pura Habilidade)
    public double BaseDefense { get; private set; }
    public double BaseMidfield { get; private set; }
    public double BaseAttack { get; private set; }

    public double OffensiveEfficiency { get; private set; } // Eficiência dinâmica
    public double RawEfficiency { get; private set; }       // Valor antes do clamp

    public double PostureMultiplier { get; private set; } // Multiplicador de tática
    public double EnergyMultiplier { get; private set; }  // Multiplicador de energia
    
    public double Instability { get; set; } = 0; // Instabilidade vinda do Clube

    public Team(string name, Coach coach)
    {
        Name = name;
        Coach = coach;
        CurrentTacticalPosture = coach.PreferredStyle; // Define a tática inicial conforme a preferência do técnico
    }
    
    // ... (Métodos AddPlayer, SetStartingEleven mantidos) ...

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
        
        RecalculateMatchStrength(false);
    }
    
    public void ChangeTacticalPosture(TacticalPosture newPosture)
    {
        CurrentTacticalPosture = newPosture;
        RecalculateMatchStrength(false);
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
        
        RecalculateMatchStrength(true);
        return true;
    }

    public void UpdateOffensiveEfficiency(double opponentDefenseStrength)
    {
        // Fórmula dinâmica:
        // (Ataque / (Ataque + DefesaOponente)) * (0.9 + Energy / 500) * Postura
        
        double totalPower = AttackStrength + opponentDefenseStrength;
        if (totalPower == 0) totalPower = 1;

        double ratio = AttackStrength / totalPower; // Ex: 0.5 em jogo equilibrado
        
        // Novo fator de energia: 0.9 + (Energy / 500). Ex: 100% -> 1.1, 50% -> 1.0
        double energyFactor = 0.9 + (GetAverageEnergy() / 500.0); 

        double postureFactor = 1.0;
        switch (CurrentTacticalPosture)
        {
            case TacticalPosture.VeryDefensive: postureFactor = 0.85; break;
            case TacticalPosture.Defensive: postureFactor = 0.95; break;
            case TacticalPosture.Balanced: postureFactor = 1.0; break;
            case TacticalPosture.Offensive: postureFactor = 1.1; break;
            case TacticalPosture.AllOutAttack: postureFactor = 1.2; break;
        }

        // Multiplicamos por 2.0 para normalizar o ratio de 0.5 para 1.0
        double rawEfficiency = ratio * energyFactor * postureFactor * 2.0;
        
        // Aplicar penalidade de Instabilidade
        // FinalEfficiency = FinalEfficiency × (1 - TeamInstability / 500)
        double instabilityPenalty = 1.0 - (Instability / 500.0);
        rawEfficiency *= instabilityPenalty;
        
        // Guardar valor bruto para logs
        RawEfficiency = rawEfficiency;
        
        // Clamp 0.80 a 1.20
        double calculated = rawEfficiency;
        if (calculated < 0.80) calculated = 0.80;
        if (calculated > 1.20) calculated = 1.20;

        OffensiveEfficiency = calculated;
    }

    public void RecalculateMatchStrength(bool isSubstitution = false)
    {
        if (StartingEleven.Count == 0) 
        {
            MatchStrength = 0;
            return;
        }

        // 1. BaseStrength Contabilizada
        BaseStrength = StartingEleven.Sum(p => p.OverallRating);

        // 2. Multiplicadores
        PostureMultiplier = 1.0;
        switch (CurrentTacticalPosture)
        {
            case TacticalPosture.VeryDefensive: PostureMultiplier = 0.7; break;
            case TacticalPosture.Defensive: PostureMultiplier = 0.85; break;
            case TacticalPosture.Balanced: PostureMultiplier = 1.0; break;
            case TacticalPosture.Offensive: PostureMultiplier = 1.15; break;
            case TacticalPosture.AllOutAttack: PostureMultiplier = 1.3; break;
        }

        EnergyMultiplier = 0.6 + (GetAverageEnergy() / 250.0);

        // 3. Setores (Com Energia Linear)
        double energyFactor = GetAverageEnergy() / 100.0;

        // Revertendo para lógica explícita de Base * Factor para garantir o efeito visual "428 -> 203"
        BaseDefense = StartingEleven.Where(p => p.Position == Position.Goalkeeper || p.Position == Position.Defender).Sum(p => p.GetEffectiveOverall());
        BaseMidfield = StartingEleven.Where(p => p.Position == Position.Midfielder).Sum(p => p.GetEffectiveOverall());
        BaseAttack = StartingEleven.Where(p => p.Position == Position.Forward).Sum(p => p.GetEffectiveOverall());

        DefenseStrength = BaseDefense * energyFactor;
        MidfieldStrength = BaseMidfield * energyFactor;
        AttackStrength = BaseAttack * energyFactor;

        // Evitar zeros
        if (DefenseStrength < 1) DefenseStrength = 10;
        if (MidfieldStrength < 1) MidfieldStrength = 10;
        if (AttackStrength < 1) AttackStrength = 10;


        // b. Offensive Efficiency: Apenas inicialização segura, pois será atualizada pelo MatchEngine
        if (OffensiveEfficiency == 0) OffensiveEfficiency = 1.0;

        // 4. Force Calculation
        double calculatedStrength = BaseStrength * PostureMultiplier * EnergyMultiplier;

        // 5. Smoothing
        if (isSubstitution)
        {
            MatchStrength = MatchStrength + ((calculatedStrength - MatchStrength) * 0.5);
        }
        else
        {
            MatchStrength = calculatedStrength;
        }
    }

    public double GetAverageEnergy()
    {
        if (StartingEleven.Count == 0) return 0;
        return StartingEleven.Average(p => p.Energy);
    }
}
