using iFootManager.Core;
using iFootManager.Core.Engine;
using iFootManager.Core.Entities;

// 1. Configurar Times e Técnicos
var homeCoach = new Coach("Pep Guardiola", TacticalPosture.Offensive);
var homeTeam = new Team("Manchester City", homeCoach);

var awayCoach = new Coach("Jurgen Klopp", TacticalPosture.Balanced);
var awayTeam = new Team("Liverpool", awayCoach);

// 2. Configurar Jogadores (Simplificado)
for (int i = 1; i <= 11; i++)
{
    homeTeam.AddPlayer(new Player($"Jogador Casa {i}", Position.Midfielder, 85));
    awayTeam.AddPlayer(new Player($"Jogador Visitante {i}", Position.Midfielder, 84));
}

// Adicionar reservas
homeTeam.AddPlayer(new Player("Reserva Casa 1", Position.Forward, 80));
awayTeam.AddPlayer(new Player("Reserva Visitante 1", Position.Forward, 79));

// Definir Titulares (Pegar os 11 primeiros por enquanto)
homeTeam.SetStartingEleven(homeTeam.Players.GetRange(0, 11));
awayTeam.SetStartingEleven(awayTeam.Players.GetRange(0, 11));

// 3. Inicializar Engine
var engine = new MatchEngine(homeTeam, awayTeam);

Console.WriteLine($"Iniciando Partida: {homeTeam.Name} vs {awayTeam.Name}");
Console.WriteLine($"Táticas: {homeTeam.CurrentTacticalPosture} vs {awayTeam.CurrentTacticalPosture}");
Console.WriteLine("--------------------------------------------------");

// 4. Game Loop
int currentMinute = 0;
while (currentMinute <= 90)
{
    engine.AdvanceMinute();
    var state = engine.GetState();
    currentMinute = state.CurrentMinute;

    // Exibir Eventos deste minuto
    var lastEvent = state.Events.LastOrDefault();
    if (lastEvent != null && lastEvent.StartsWith($"[{currentMinute}']"))
    {
        Console.WriteLine(lastEvent);
    }
    
    // Lógica simples de substituição (Exemplo no minuto 60)
    if (currentMinute == 60)
    {
         engine.ApplySubstitution(true, homeTeam.StartingEleven[0], homeTeam.Bench[0]);
    }
    
    // Thread.Sleep(100); // Simulando delay se necessário
}

Console.WriteLine("--------------------------------------------------");
Console.WriteLine($"Placar Final: {homeTeam.Name} {engine.GetState().HomeScore} - {engine.GetState().AwayScore} {awayTeam.Name}");
