using iFootManager.Core;
using iFootManager.Core.Engine;
using iFootManager.Core.Entities;

// === CONFIGURAÇÃO INICIAL DA TEMPORADA ===

// 1. Criar Clubes
var city = new Club("Man City", ClubSize.Large, "Premier League", "Título");
var liverpool = new Club("Liverpool", ClubSize.Large, "Premier League", "G-4");
var arsenal = new Club("Arsenal", ClubSize.Large, "Premier League", "G-4");
var chelsea = new Club("Chelsea", ClubSize.Medium, "Premier League", "Meio de Tabela");

var clubs = new List<Club> { city, liverpool, arsenal, chelsea };

// 2. Configurar Times e Técnicos Simplificados
SetupTeam(city, "Pep Guardiola", TacticalPosture.Offensive, CoachSpecialty.OffensiveTactician);
SetupTeam(liverpool, "Arne Slot", TacticalPosture.Balanced, CoachSpecialty.Motivator);
SetupTeam(arsenal, "Mikel Arteta", TacticalPosture.Offensive, CoachSpecialty.YouthDeveloper);
SetupTeam(chelsea, "Enzo Maresca", TacticalPosture.Defensive, CoachSpecialty.DefensiveMastermind);

// CLUBE DO USUÁRIO: Man City (para visualizar logs detalhados)
var userClub = city;

// 3. Criar Liga
var league = new League("Premier League Sim", clubs);

Console.WriteLine($"=== INICIANDO TEMPORADA: {league.Name} ===");
Console.WriteLine($"Clubes: {clubs.Count} | Rodadas: {league.TotalRounds}");
Console.WriteLine($"Seu Clube: {userClub.Name} (Confiança: {userClub.BoardTrust:F0}%)");
Console.WriteLine(new string('=', 50));

// === GAME LOOP: RODADAS ===
while (league.CurrentRound <= league.TotalRounds)
{
    Console.WriteLine($"\n>>> RODADA {league.CurrentRound} <<<");
    var fixtures = league.GetCurrentRoundFixtures();

    foreach (var match in fixtures)
    {
        bool isUserMatch = (match.Home == userClub || match.Away == userClub);
        
        // Simular Partida
        var engine = new MatchEngine(match.Home.Squad, match.Away.Squad);
        
        // Se for jogo do usuário, dar feedback visual mínimo durante a partida (ou pular para resultado)
        // Para este teste, vamos pular logs minuto a minuto para não spammar, mostrando apenas placar final.
        // Exceto se for jogo do usuário, onde mostraremos highlights rápidos.
        
        if (isUserMatch) Console.WriteLine($"\n[JOGO DO USUÁRIO] {match.Home.Name} vs {match.Away.Name}");

        while (engine.GetState().CurrentMinute <= 90)
        {
            engine.AdvanceMinute();
            // Se quiser logs detalhados:
            // if (isUserMatch && engine.GetState().CurrentMinute % 45 == 0) ...
        }

        var result = engine.GetState();
        league.ProcessMatchResult(result, match.Home, match.Away);

        Console.WriteLine($"FIM: {match.Home.Name} {result.HomeScore} - {result.AwayScore} {match.Away.Name}");
        
        // Avaliação de Carreira (Se for user)
        if (match.Home == userClub) userClub.EvaluateMatch(result, true);
        if (match.Away == userClub) userClub.EvaluateMatch(result, false);
    }

    // Mostrar Tabela Atualizada
    PrintTable(league);
    
    // Status do Usuário
    Console.WriteLine($"[STATUS: {userClub.Name}]");
    Console.WriteLine($"Confiança: {userClub.BoardTrust:F0}% | Especialidade ({userClub.Squad.Coach.PrimarySpecialty}): {userClub.Squad.Coach.PrimarySpecialtyStrength:F0}");
    
    var msgAvgMorale = userClub.Squad.Players.Average(p => p.Morale);
    var bestMorale = userClub.Squad.Players.OrderByDescending(p => p.Morale).First();
    var worstMorale = userClub.Squad.Players.OrderBy(p => p.Morale).First();
    
    Console.WriteLine($"Moral Média: {msgAvgMorale:F1} | Top: {bestMorale.Name} ({bestMorale.Morale:F0}) | Baixo: {worstMorale.Name} ({worstMorale.Morale:F0})");

    string lockerRoomStatus = "Estável";
    if (userClub.TeamInstability > 20) lockerRoomStatus = "Tenso";
    if (userClub.TeamInstability > 50) lockerRoomStatus = "CRÍTICO";
    
    Console.WriteLine($"Vestiário: {lockerRoomStatus} (Instabilidade: {userClub.TeamInstability:F0})");

    if (userClub.UnderPressure) Console.WriteLine("!!! ALERTA: TÉCNICO SOB PRESSÃO !!!");
    
    Console.WriteLine("Pressione ENTER para próxima rodada...");
    // Console.ReadLine(); // Comentado para automação, descomentar para interatividade real
    
    league.AdvanceRound();
}

Console.WriteLine("\n=== FIM DA TEMPORADA ===");


// --- HELPER METHODS ---

void SetupTeam(Club club, string coachName, TacticalPosture style, CoachSpecialty specialty)
{
    var coach = new Coach(coachName, style, specialty);
    var team = new Team(club.Name, coach);
    
    // Elenco Genérico Forte para testar a liga
    team.AddPlayer(new Player("GK", Position.Goalkeeper, 85));
    for(int i=0; i<4; i++) team.AddPlayer(new Player($"DEF {i}", Position.Defender, 82));
    for(int i=0; i<3; i++) team.AddPlayer(new Player($"MID {i}", Position.Midfielder, 84));
    for(int i=0; i<3; i++) team.AddPlayer(new Player($"FWD {i}", Position.Forward, 86));
    
    // Reservas
    team.AddPlayer(new Player("SUB FWD", Position.Forward, 80));
    team.AddPlayer(new Player("SUB MID", Position.Midfielder, 80));

    team.SetStartingEleven(team.Players.GetRange(0, 11));
    
    club.Squad = team;
}

void PrintTable(League league)
{
    Console.WriteLine("\nCLASSIFICAÇÃO:");
    Console.WriteLine($"{"Pos",-3} {"Clube",-15} {"P",-3} {"J",-3} {"V",-3} {"E",-3} {"D",-3} {"SG",-3}");
    Console.WriteLine(new string('-', 50));
    
    var standings = league.GetStandings();
    int pos = 1;
    foreach (var entry in standings)
    {
        Console.WriteLine($"{pos,-3} {entry.Club.Name,-15} {entry.Points,-3} {entry.Played,-3} {entry.Won,-3} {entry.Drawn,-3} {entry.Lost,-3} {entry.GoalDifference,-3}");
        pos++;
    }
    Console.WriteLine(new string('-', 50));
}
