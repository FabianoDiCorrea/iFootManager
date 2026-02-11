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

        // Lógica simples de simulação
        // Chance de evento baseada em fatores (aleatório por enquanto)
        int roll = _random.Next(1, 101);

        // Ajustar chances baseadas na Postura Tática (Exemplo Simples)
        // Ofensivo = maior chance de marcar mas também maior chance de sofrer (não totalmente implementado, apenas ilustrativo)
        
        // 5% de chance de um evento de gol para demonstração
        if (roll <= 5)
        {
            // Determina quem marcou (50/50 por enquanto)
            bool homeScored = _random.Next(0, 2) == 0;
            string scorer = homeScored ? "Jogador da Casa" : "Jogador Visitante"; // Placeholder
            
            _state.AddGoal(homeScored, $"GOL! {scorer} marcou!");
        }
        else if (roll > 95)
        {
            _state.AddEvent("Um chute perigoso foi defendido!");
        }
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
