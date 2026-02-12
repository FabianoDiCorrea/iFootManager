using iFootManager.Core.Entities;

namespace iFootManager.Core.Engine;

// Controlador principal da simulação da partida
public class MatchEngine
{
    private readonly MatchState _state;
    private readonly Random _random;

    public MatchEngine(Team homeTeam, Team awayTeam)
    {
        _state = new MatchState(homeTeam, awayTeam);
        _random = new Random();
    }

    public MatchState GetState() => _state;

    public void AdvanceMinute()
    {
        _state.IncrementMinute();

        // 1. Atualizar Energia e Recalcular Força
        UpdateTeamStatus(_state.HomeTeam);
        UpdateTeamStatus(_state.AwayTeam);
        
        // 2. Atualizar Eficiências Cruzadas
        UpdateEfficiencies();

        // === FASE 1: CRIAÇÃO DE JOGADA (Midfield vs Midfield) ===
        // Base Attack Rate: 20% (0.20)
        double baseAttackRate = 0.20;

        // Processar ataque para CASA
        ProcessAttackPhase(_state.HomeTeam, _state.AwayTeam, baseAttackRate, true);

        // Processar ataque para VISITANTE
        ProcessAttackPhase(_state.AwayTeam, _state.HomeTeam, baseAttackRate, false);
    }

    private void ProcessAttackPhase(Team attacker, Team defender, double baseRate, bool isHome)
    {
        // Força do Meio-Campo
        double totalMidfield = attacker.MidfieldStrength + defender.MidfieldStrength;
        if (totalMidfield == 0) totalMidfield = 1;

        double midRatio = attacker.MidfieldStrength / totalMidfield;

        // Multiplicador de postura (Defensive cria menos, Offensive cria mais)
        double postureMult = 1.0;
        switch (attacker.CurrentTacticalPosture)
        {
            case TacticalPosture.VeryDefensive: postureMult = 0.6; break;
            case TacticalPosture.Defensive: postureMult = 0.8; break;
            case TacticalPosture.Balanced: postureMult = 1.0; break;
            case TacticalPosture.Offensive: postureMult = 1.2; break;
            case TacticalPosture.AllOutAttack: postureMult = 1.4; break;
        }

        // Chance Final de Criar Ataque
        double creationChance = midRatio * postureMult * baseRate;

        // Rolagem
        if (_random.NextDouble() < creationChance)
        {
            // === FASE 2: CONVERSÃO (Attack vs Defense) ===
            double attackPower = attacker.AttackStrength;
            double defensePower = defender.DefenseStrength;
            
            // Postura defensiva do oponente aumenta a defesa dele? 
            if (defender.CurrentTacticalPosture == TacticalPosture.Defensive) defensePower *= 1.1;
            if (defender.CurrentTacticalPosture == TacticalPosture.VeryDefensive) defensePower *= 1.25;

            double totalEndGame = attackPower + defensePower;
            double conversionRatio = attackPower / totalEndGame; // Ex: 0.5

            // Modificador de Finalização (Reduzido para 0.25 para metas de 2-4 gols)
            double finishingModifier = 0.25; 

            // Chance Base
            double baseGoalChance = conversionRatio * finishingModifier;
            
            // Aplicar Eficiência Ofensiva (0.85 - 1.15)
            double efficiencyMult = attacker.OffensiveEfficiency;
            double adjustedChance = baseGoalChance * efficiencyMult;

            // Aplicar Cool-down de Gols em Sequência
            // Se marcou nos últimos 3 minutos, reduz chance
            int lastGoalMinute = isHome ? _state.LastHomeGoalMinute : _state.LastAwayGoalMinute;
            bool isCoolDown = (_state.CurrentMinute - lastGoalMinute) <= 3;
            double coolDownMult = isCoolDown ? 0.7 : 1.0;

            double finalGoalChance = adjustedChance * coolDownMult;

            if (_random.NextDouble() < finalGoalChance)
            {
                 // GOL!
                 // Selecionar um jogador aleatório do ataque para ser o autor do gol (simplificação)
                 var scorer = attacker.StartingEleven.First(p => p.Position == Position.Forward); // Pega o primeiro atacante por enquanto
                 
                 string teamName = isHome ? "CASA" : "VISITANTE";
                 // Log super detalhado para debug (inclui nome do jogador)
                 _state.AddGoal(isHome, scorer, $"GOL! {scorer.Name} ({teamName}) marcou! (Prob: {finalGoalChance:P1})");
            }
            else
            {
                 // Perdeu gol / Defesa
                 if (_random.NextDouble() > 0.6) 
                    _state.AddChance(isHome, $"Uh! {attacker.Name} chegou! (Prob: {finalGoalChance:P1})");
            }
        }
    }

    private void UpdateTeamStatus(Team team)
    {
        foreach (var player in team.StartingEleven)
        {
            player.UpdateEnergy(team.CurrentTacticalPosture);
        }
        // Atualiza força e setores
        team.RecalculateMatchStrength(false);
    }
    
    private void UpdateEfficiencies()
    {
        // Atualiza eficiência ofensiva baseada na defesa do OPONENTE
        _state.HomeTeam.UpdateOffensiveEfficiency(_state.AwayTeam.DefenseStrength);
        _state.AwayTeam.UpdateOffensiveEfficiency(_state.HomeTeam.DefenseStrength);
    }

    public void ApplySubstitution(bool isHomeTeam, Player playerOut, Player playerIn)
    {
        var team = isHomeTeam ? _state.HomeTeam : _state.AwayTeam;
        if (team.ApplySubstitution(playerOut, playerIn))
        {
            _state.AddEvent($"Substituição no {team.Name}: SAI {playerOut.Name}, ENTRA {playerIn.Name}.");
        }
        else
        {
            _state.AddEvent($"Substituição falhou para {team.Name}.");
        }
    }

    public void ChangeTacticalPosture(bool isHomeTeam, TacticalPosture newPosture)
    {
        var team = isHomeTeam ? _state.HomeTeam : _state.AwayTeam;
        team.ChangeTacticalPosture(newPosture);
        _state.AddEvent($"{team.Name} mudou a tática para {newPosture}.");
    }
}
