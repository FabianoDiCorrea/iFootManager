using iFootManager.Core.Engine;
using iFootManager.Core; // Importante para Enums

namespace iFootManager.Core.Entities;

public class Club
{
    public string Name { get; private set; }
    public string Division { get; private set; }
    public int SeasonsInCurrentDivision { get; private set; }
    public ClubSize Size { get; private set; }
    public double BoardTrust { get; private set; } = 70;
    public string SeasonObjective { get; private set; }
    
    public double TeamInstability { get; private set; } = 0; // 0 a 100
    public int UnbeatenStreak { get; private set; } = 0;
    
    public Team Squad { get; set; } // Link para o time (jogadores/técnico)
    
    public bool UnderPressure => BoardTrust < 30;

    public Club(string name, ClubSize size, string division, string objective)
    {
        Name = name;
        Size = size;
        Division = division;
        SeasonObjective = objective;
    }

    public void EvaluateMatch(MatchState match, bool isHome)
    {
        Team myTeam = isHome ? match.HomeTeam : match.AwayTeam;
        int myScore = isHome ? match.HomeScore : match.AwayScore;
        int oppScore = isHome ? match.AwayScore : match.HomeScore;
        int myChances = isHome ? match.HomeChances : match.AwayChances;
        
        // 1. Result Score
        double resultScore = 0;
        if (myScore > oppScore) 
        {
            resultScore = 3;       // Vitória
            UnbeatenStreak++;
        }
        else if (myScore == oppScore) 
        {
            resultScore = 1; // Empate
            UnbeatenStreak++;
        }
        else 
        {
            resultScore = -3;      // Derrota
            UnbeatenStreak = 0;
        }
        
        // 2. Performance Score (Baseado apenas em gols por enquanto, ou saldo)
        // Se ganhou de goleada +1
        if (myScore - oppScore >= 3) resultScore += 1;
        
        // 3. Coherence Score (Coach Specialty)
        double coherenceScore = 0;
        bool coherent = false;
        
        switch (myTeam.Coach.PrimarySpecialty)
        {
            case CoachSpecialty.OffensiveTactician:
                // Coerente se criar muitas chances (> 5) ou fizer gols (> 1)
                if (myChances >= 5 || myScore >= 2) 
                {
                    coherent = true;
                    coherenceScore = 2;
                }
                else
                {
                    // Incoerente se criar pouco
                    coherenceScore = -2;
                }
                break;
                
            case CoachSpecialty.DefensiveMastermind:
                // Coerente se sofrer poucos gols (<= 1)
                if (oppScore <= 1)
                {
                    coherent = true;
                    coherenceScore = 2;
                }
                else
                {
                    coherenceScore = -2;
                }
                break;
                
            default:
                // Balanced/Motivator: Coerente se não perder feio?
                if (myScore >= oppScore - 1) coherent = true;
                break;
        }

        // Atualizar Trust Inicial
        double trustChange = resultScore + coherenceScore;
        BoardTrust += trustChange;
        
        // Atualizar Coach Specialty
        myTeam.Coach.UpdateSpecialty(coherent);
        
        // 4. Atualizar Moral dos Jogadores e INSTABILIDADE
        int playersWithLowMorale = 0;

        foreach (var player in myTeam.Players)
        {
            double moraleChange = 0;
            
            // Base por Resultado
            if (resultScore == 3) moraleChange += 2;      // Vitória
            else if (resultScore == 1) moraleChange += 1; // Empate
            else moraleChange -= 3;                       // Derrota
            
            // Bônus por Gol
            if (match.GoalScorers.ContainsKey(player))
            {
                moraleChange += (match.GoalScorers[player] * 3);
            }
            
            // Compatibilidade Tática (Simplificada)
            if (myTeam.Coach.PreferredStyle == TacticalPosture.Offensive && player.Position == Position.Forward)
                moraleChange += 1;

            if (myTeam.Coach.PreferredStyle == TacticalPosture.Defensive && player.Position == Position.Defender)
                moraleChange += 1;
                
            player.UpdateMorale(moraleChange);
            
            // Verificar Instabilidade Individual
            if (player.Morale < 30)
            {
                playersWithLowMorale++;
                player.CheckLowMoraleStreak(); 
                
                // Se jogador ficar 3 partidas seguidas com Morale < 30: +2 Instabilidade
                if (player.LowMoraleStreak >= 3)
                {
                    TeamInstability += 2;
                }
            }
            else
            {
                player.CheckLowMoraleStreak(); // Reseta
            }
        }
        
        // Regra de Contágio (2 ou mais jogadores com moral baixa)
        if (playersWithLowMorale >= 2)
        {
            TeamInstability += 3;
        }
        
        // --- Regras de Redução de Instabilidade ---
        
        // 1. Vitória
        if (resultScore >= 3) TeamInstability -= 5; 
        
        // 2. Sequência Invicta (3 jogos)
        if (UnbeatenStreak >= 3) TeamInstability -= 3;
        
        // 3. Técnico Motivador
        if (myTeam.Coach.PrimarySpecialty == CoachSpecialty.Motivator && 
            myTeam.Coach.PrimarySpecialtyStrength > 90)
        {
            TeamInstability -= 2;
        }
        
        // Média de Moral ajuda também
        double avgMorale = myTeam.Players.Average(p => p.Morale);
        if (avgMorale > 75) TeamInstability -= 2; 

        // Clamp Instability (0 - 100)
        if (TeamInstability < 0) TeamInstability = 0;
        if (TeamInstability > 100) TeamInstability = 100;
        
        // --- Impacto da Instabilidade na Diretoria ---
        if (TeamInstability > 40)
        {
            double penalty = TeamInstability / 20.0;
            BoardTrust -= penalty; // Ex: 50 -> -2.5
        }
        
        // Clamp BoardTrust (0 - 100)
        if (BoardTrust < 0) BoardTrust = 0;
        if (BoardTrust > 100) BoardTrust = 100;

        // Sincronizar com o Time (para afetar a engine)
        myTeam.Instability = TeamInstability;
        
        // Identificar Status do Vestiário para log
        string lockerStatus = "Estável";
        if (TeamInstability > 20) lockerStatus = "Tenso";
        if (TeamInstability > 50) lockerStatus = "CRÍTICO";

        Console.WriteLine($"[AVALIAÇÃO DA DIRETORIA]");
        Console.WriteLine($"Resultado: {resultScore:+0;-0} | Coerência: {coherenceScore:+0;-0}");
        if (TeamInstability > 40) Console.WriteLine($"! PENALIDADE POR INSTABILIDADE ! (-{TeamInstability/20.0:F1} na Confiança)");
        Console.WriteLine($"Confiança Atual: {BoardTrust:F0} (Pressão: {UnderPressure})");
        Console.WriteLine($"Especialidade Técnico ({myTeam.Coach.PrimarySpecialty}): {myTeam.Coach.PrimarySpecialtyStrength:F0}");
        Console.WriteLine($"Vestiário: {lockerStatus} ({TeamInstability:F0}) | Invencibilidade: {UnbeatenStreak}");
        Console.WriteLine("--------------------------------------------------");
    }
}
