using iFootManager.Core;
using iFootManager.Core.Engine;
using iFootManager.Core.Entities;
using iFootManager.Simulator.UI;

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

// Configurar Rivalidades (Exemplo)
city.SetRivalry("Liverpool", 85);
city.SetRivalry("Arsenal", 75); // Disputa recente
city.SetRivalry("Chelsea", 60);

liverpool.SetRivalry("Man City", 85);
liverpool.SetRivalry("Chelsea", 70);

arsenal.SetRivalry("Chelsea", 80); // Derby de Londres
arsenal.SetRivalry("Man City", 75);

chelsea.SetRivalry("Arsenal", 80);
chelsea.SetRivalry("Liverpool", 70);

// CLUBE DO USUÁRIO: Man City (para visualizar logs detalhados)
var userClub = city;

// 3. Criar Liga
var league = new League("Premier League Sim", clubs);

ConsoleUI.DrawHeader($"INICIANDO TEMPORADA: {league.Name}", $"Clubes: {clubs.Count} | Rodadas: {league.TotalRounds}");
Console.WriteLine("\nPressione ENTER para começar...");
// Console.ReadLine();

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
        
        if (isUserMatch) 
        {
            Console.WriteLine($"\n[JOGO DO USUÁRIO] {match.Home.Name} vs {match.Away.Name}");
            // Identificar Jogo Grande
            int rivalry = match.Home.GetRivalryLevel(match.Away.Name);
            bool isBigMatch = rivalry > 70;
            
            if (isBigMatch)
            {
                ConsoleUI.DrawCard("⚠️ ALERTA PRÉ-JOGO", new List<string>
                {
                    $"{ConsoleUI.GetBadge("BigMatch")} CLÁSSICO DETECTADO!",
                    $"Rivalidade: {rivalry}/100",
                    "A tensão nos bastidores é alta. A vitória vale muito mais hoje!"
                }, ConsoleUI.ColorWarning);
                
                // Impacto Emocional Pré-Jogo
                userClub.CoachPressure += 3;
                if (userClub.TeamInstability > 40) userClub.TeamInstability += 2;
            }
        }

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
        if (match.Home == userClub) 
        {
            bool isBig = match.Home.GetRivalryLevel(match.Away.Name) > 70;
            userClub.ProcessMatchRevenue(result); // Bilheteria
            userClub.EvaluateMatch(result, true, isBig);
        }
        if (match.Away == userClub) 
        {
            bool isBig = match.Away.GetRivalryLevel(match.Home.Name) > 70;
            userClub.EvaluateMatch(result, false, isBig);
        }
    }
    
    // Simular Passagem do Mês (A cada 4 Rodadas)
    if (league.CurrentRound % 4 == 0)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n[FIM DE MÊS] Processando pagamentos e mercado...");
        Console.ResetColor();
        userClub.ProcessMonthlyFinancials();
        
        // Determinar status para mercado
        var stand = league.GetStandings();
        var myEntry = stand.FirstOrDefault(e => e.Club == userClub);
        string status = "Normal";
        if (myEntry != null)
        {
             int p = stand.IndexOf(myEntry) + 1;
             if (p == 1) status = "Leader";
        }
        userClub.ProcessTransferMarket(status);
    }

    // Mostrar Tabela Atualizada
    PrintTable(league);

    // Avaliação de Expectativa (Pós Tabela)
    var standings = league.GetStandings();
    var userEntry = standings.FirstOrDefault(e => e.Club == userClub);
    int pos = 0;
    if (userEntry != null)
    {
        pos = standings.IndexOf(userEntry) + 1;
        userClub.EvaluateExpectation(pos);
    }
    
    // --- DASHBOARD PRINCIPAL ---
    
    // Header com Avatar e Info Básica
    ConsoleUI.DrawHeader($"{userClub.Name.ToUpper()} [{userClub.Division}]", $"Rodada {league.CurrentRound-1} Finalizada");
    
    string coachFeeling = "Happy";
    if (userClub.CoachPressure > 50) coachFeeling = "Neutral";
    if (userClub.CoachPressure > 80) coachFeeling = "Angry";
    
    ConsoleUI.DrawAvatar(userClub.Squad.Coach.Name, "Técnico", coachFeeling);
    Console.WriteLine();

    // CARD 1: STATUS
    var statusLines = new List<string>
    {
        $"{ConsoleUI.Icons.Coach} Estilo: {userClub.Squad.Coach.Style} | Fit: {userClub.Squad.TacticalFit:F1}%",
        $"Objetivo: {userClub.SeasonObjective} | Status: {userClub.ExpectationStatus}",
        $"Posição Atual: {pos}º Lugar"
    };
    if (userClub.ExpectationStatus == "Abaixo do esperado") statusLines.Insert(0, $"{ConsoleUI.Icons.Alert} {ConsoleUI.GetBadge("LastChance")}");
    
    ConsoleUI.DrawCard("STATUS DO CLUBE", statusLines, ConsoleUI.Colors.Primary, ConsoleUI.CardStyle.Double);

    // CARD 2: INDICADORES (com animação)
    Console.WriteLine(); 
    ConsoleUI.DrawProgressBar("Confiança", userClub.BoardTrust, 100, userClub.BoardTrust < 40 ? ConsoleUI.Colors.Danger : ConsoleUI.Colors.Success, animate: true);
    ConsoleUI.DrawProgressBar("Pressão", userClub.CoachPressure, 100, userClub.CoachPressure > 70 ? ConsoleUI.Colors.Danger : ConsoleUI.Colors.Success); // Sem animação para não demorar tanto
    ConsoleUI.DrawProgressBar("Instabilidade", userClub.TeamInstability, 100, userClub.TeamInstability > 50 ? ConsoleUI.Colors.Danger : ConsoleUI.Colors.Success);
    Console.WriteLine();

    // CARD 3: FINANÇAS
    var financeColor = userClub.Balance < 0 ? ConsoleUI.Colors.Danger : ConsoleUI.Colors.Success;
    var financeLines = new List<string>
    {
        $"{ConsoleUI.Icons.Money} Saldo: {ConsoleUI.FormatCurrency(userClub.Balance)}",
        $"Folha Salarial: {ConsoleUI.FormatCurrency(userClub.MonthlyWageBill)}/mês",
        $"Status: {userClub.FinancialStatus}"
    };
    ConsoleUI.DrawCard("FINANÇAS", financeLines, financeColor, ConsoleUI.CardStyle.Modern);

    // CARD 4: VESTIÁRIO
    
    var leaders = userClub.Squad.Players.Where(p => p.LockerRoomInfluence > 70).OrderByDescending(p => p.LockerRoomInfluence).Take(3);
    var unhappy = userClub.Squad.Players.Where(p => p.Morale < 40).OrderBy(p => p.Morale).Take(3);
    
    var lockerLines = new List<string>();
    lockerLines.Add($"Moral Média: {userClub.Squad.Players.Average(p => p.Morale):F1}");
    lockerLines.Add("--- LÍDERES ---");
    foreach(var l in leaders) lockerLines.Add($"{ConsoleUI.IconStar} {l.Name} (Inf:{l.GetInfluenceLevelStatus()})");
    
    if (unhappy.Any())
    {
        lockerLines.Add("--- INSATISFEITOS ---");
        foreach(var u in unhappy) lockerLines.Add($"{ConsoleUI.IconSkull} {u.Name} (Moral:{u.Morale:F0})");
    }
    else
    {
        lockerLines.Add($"{ConsoleUI.IconSuccess} Ninguém insatisfeito."); 
    }
    
    ConsoleUI.DrawCard("VESTIÁRIO", lockerLines, ConsoleUI.ColorDefault);

    // ALERTAS
    if (userClub.CoachUnderUltimatum)
    {
        ConsoleUI.DrawCard("!!! ULTIMATO !!!", new List<string>{"VITÓRIA OBRIGATÓRIA NO PRÓXIMO JOGO OU DEMISSÃO!"}, ConsoleUI.ColorDanger);
    }
    else if (userClub.CrisisRisk > 0)
    {
        ConsoleUI.DrawCard("ALERTA DE CRISE", new List<string>{$"Risco Nível: {userClub.CrisisRisk}"}, ConsoleUI.ColorWarning);
    }

    // EVENTOS
    if (userClub.EventLog.Any())
    {
        ConsoleUI.DrawCard("NOTÍCIAS E EVENTOS", userClub.EventLog, ConsoleUI.ColorDefault);
        userClub.EventLog.Clear();
    }
    
    Console.WriteLine("\nPressione ENTER para próxima rodada...");
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
