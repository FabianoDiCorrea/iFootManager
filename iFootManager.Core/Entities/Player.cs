using iFootManager.Core;

namespace iFootManager.Core.Entities;

// Representa um jogador de futebol
public class Player
{
    public string Name { get; private set; }
    public Position Position { get; private set; }
    public int OverallRating { get; private set; } // Habilidade base (0-100)
    public double Energy { get; private set; } = 100; // Energia (0-100)
    public double Morale { get; private set; } = 80;
    
    // Atributos Detalhados (para Filosofia Tática)
    public int Age { get; private set; }
    public int Technical { get; private set; }
    public int Physical { get; private set; }
    public int Mental { get; private set; }
    
    // Mercado de Transferências
    public int StayDesire { get; private set; } = 100; // 0-100 (Desejo de ficar)
    public decimal MarketValue { get; private set; }   // Valor de Mercado Estimado

    // Rastreamento de Moral
    public int LockerRoomInfluence { get; private set; } = 50;
    public int LowMoraleStreak { get; private set; } = 0;

    public bool IsCaptain { get; private set; }
    public bool IsIdol { get; private set; }

    public Player(string name, Position position, int overallRating)
    {
        Name = name;
        Position = position;
        OverallRating = overallRating;
        
        // Simular Hierarquia Inicial
        CalculateInfluence();
        InitializeAttributes();
    }

    private void InitializeAttributes()
    {
        Random rnd = new Random();
        
        // Idade (18 a 38, ponderado)
        Age = rnd.Next(18, 36);
        if (OverallRating > 90) Age = rnd.Next(24, 34); // Estrelas geralmente auge
        
        // Distribuição baseada em Posição e Overall
        // Variação em torno do Overall (+- 10)
        
        int baseStat = OverallRating;
        Technical = Math.Clamp(baseStat + rnd.Next(-10, 10), 40, 99);
        Physical = Math.Clamp(baseStat + rnd.Next(-10, 10), 40, 99);
        Mental = Math.Clamp(baseStat + rnd.Next(-10, 10), 40, 99);
        
        // Ajustes por Posição
        if (Position == Position.Forward) Technical += 5;
        if (Position == Position.Defender) Mental += 5; // Concentração
        if (Position == Position.Midfielder) Technical += 2;
        
        // Ajustes por Idade
        if (Age > 32) Physical -= 10;
        if (Age < 20) Mental -= 10;
        
        // Re-Clamp
        Technical = Math.Clamp(Technical, 1, 99);
        Physical = Math.Clamp(Physical, 1, 99);
        Mental = Math.Clamp(Mental, 1, 99);
    }

    
    private void CalculateInfluence()
    {
        // Base na Habilidade
        LockerRoomInfluence = (int)(OverallRating * 0.5);
        
        // Fator Aleatório (Personalidade/Tempo de Casa)
        LockerRoomInfluence += new Random().Next(0, 30);
        
        // Ídolo (Se Overall > 90, chance 30%)
        if (OverallRating > 90 && new Random().NextDouble() < 0.3)
        {
            IsIdol = true;
            LockerRoomInfluence += 15;
        }
        
        // Capitania (Para fins de simulação, vamos definir fora ou aleatório se ninguem for)
        // Por simplificação: Se for Ídolo ou tiver Overall > 95, vira Capitão (um por time idealmente, mas aqui ok)
        // Vamos deixar IsCaptain settable externamente ou chance baixa aqui? 
        // Vamos dar chance baixa se for veterano (não temos idade, usa overall como proxy de exp)
        if (!IsIdol && OverallRating > 85 && new Random().NextDouble() < 0.1)
        {
            IsCaptain = true;
            LockerRoomInfluence += 20;
        }

        if (LockerRoomInfluence > 100) LockerRoomInfluence = 100;
    }

    public string GetInfluenceLevelStatus()
    {
        if (LockerRoomInfluence > 85) return "Líder";
        if (LockerRoomInfluence > 70) return "Influente";
        if (LockerRoomInfluence > 40) return "Média";
        return "Baixa";
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
    
    // --- LÓGICA DE MERCADO ---

    public void CalculateMarketValue(string clubStatus) // clubStatus: "Champion", "Leader", "Normal"
    {
        // Base Value: Overall^3 * 10
        // Ex: 80 -> 512,000 * 10 = 5.1M
        // Ex: 90 -> 729,000 * 10 = 7.2M (ajustar constantes para realismo)
        
        // Ajuste de constantes para valores mais realistas
        // 80 -> ~20M
        // 90 -> ~80M
        
        double baseVal = Math.Pow(OverallRating, 3) * 40; 
        
        // Multiplicador de Idade
        double ageMultiplier = 1.0;
        if (Age < 22) ageMultiplier = 1.5; // Jovem promessa
        else if (Age < 26) ageMultiplier = 1.2; // Auge físico
        else if (Age > 32) ageMultiplier = 0.6; // Veterano
        
        // Multiplicador de Status do Clube
        double statusMultiplier = 1.0;
        if (clubStatus == "Champion") statusMultiplier = 1.2;
        if (clubStatus == "Leader") statusMultiplier = 1.1;
        
        // Multiplicador de Moral/Fase
        double formMultiplier = 0.8 + (Morale / 250.0); // 0.8 a 1.2
        
        MarketValue = (decimal)(baseVal * ageMultiplier * statusMultiplier * formMultiplier);
    }
    
    public void UpdateStayDesire(double teamInstability, double coachPressure, string expectationStatus, bool isStarter)
    {
        double desire = 100;
        
        // 1. Instabilidade do Clube
        desire -= (teamInstability * 0.5); // Se instabilidade 50 -> -25
        
        // 2. Pressão no Técnico
        if (coachPressure > 70) desire -= 10;
        
        // 3. Expectativa
        if (expectationStatus == "Abaixo do esperado") desire -= 5;
        if (expectationStatus == "Acima do esperado") desire += 5;
        
        // 4. Moral Pessoal
        if (Morale < 40) desire -= 20;
        if (Morale > 80) desire += 10;
        
        // 5. Tempo de Jogo (Simplificado: Titular x Reserva)
        if (!isStarter) desire -= 15;
        
        // Clamp
        if (desire < 0) desire = 0;
        if (desire > 100) desire = 100;
        
        StayDesire = (int)desire;
    }
}
