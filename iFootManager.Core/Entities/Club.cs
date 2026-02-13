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
    
    public double TeamInstability { get; set; } = 0; // 0 a 100
    public int UnbeatenStreak { get; private set; } = 0;
    
    // --- Sistema de Crise ---
    public int CrisisRisk { get; private set; } = 0;
    public double CoachPressure { get; set; } = 0; // 0 a 100
    public bool CoachUnderUltimatum { get; private set; } = false;
    public int SeasonExpectation { get; private set; } // 1 a 5
    public string ExpectationStatus { get; private set; } = "Dentro do esperado"; // Texto para UI
    public List<int> RecentPoints { get; private set; } = new List<int>();

    // --- Rivalidades ---
    public Dictionary<string, int> Rivalries { get; private set; } = new Dictionary<string, int>();

    public List<string> EventLog { get; private set; } = new List<string>();

    // --- Sistema Financeiro ---
    public decimal Balance { get; private set; }
    public decimal MonthlySponsorship { get; private set; }
    public decimal MonthlyWageBill { get; private set; }
    public decimal TicketPrice { get; private set; }
    public int StadiumCapacity { get; private set; }
    public decimal LastMatchRevenue { get; private set; }
    public string FinancialStatus { get; private set; } = "Estável";
    // -------------------------

    public Team Squad { get; set; } // Link para o time (jogadores/técnico)
    
    public bool UnderPressure => BoardTrust < 30;

    public Club(string name, ClubSize size, string division, string objective)
    {
        Name = name;
        Size = size;
        Division = division;
        SeasonObjective = objective;
        
        // Mapear Objetivos para Expectativa Numérica
        if (objective == "Título") SeasonExpectation = 5;
        else if (objective == "G-4" || objective == "G4") SeasonExpectation = 4;
        else if (objective == "Parte de Cima") SeasonExpectation = 3;
        else if (objective == "Meio de Tabela") SeasonExpectation = 2;
        else SeasonExpectation = 1;

        // Inicializar Pressão Baseada na Expectativa
        if (SeasonExpectation >= 4) CoachPressure = 40;
        else if (SeasonExpectation >= 2) CoachPressure = 25;
        else CoachPressure = 15;

        InitializeFinances();
    }

    public void SetRivalry(string clubName, int level)
    {
        if (Rivalries.ContainsKey(clubName))
            Rivalries[clubName] = level;
        else
            Rivalries.Add(clubName, level);
    }

    public int GetRivalryLevel(string clubName)
    {
        return Rivalries.ContainsKey(clubName) ? Rivalries[clubName] : 0;
    }

    private void InitializeFinances()
    {
        // Configuração Financeira Básica baseada no tamanho
        if (Size == ClubSize.Large)
        {
            Balance = 100_000_000;
            MonthlySponsorship = 5_000_000;
            StadiumCapacity = 60000;
            TicketPrice = 150;
        }
        else if (Size == ClubSize.Medium)
        {
            Balance = 20_000_000;
            MonthlySponsorship = 1_500_000;
            StadiumCapacity = 30000;
            TicketPrice = 80;
        }
        else
        {
            Balance = 5_000_000;
            MonthlySponsorship = 500_000;
            StadiumCapacity = 15000;
            TicketPrice = 40;
        }

        // Estimativa Inicial de Salários (será recalculada quando tiver Squad, mas garante valor não zero)
        MonthlyWageBill = MonthlySponsorship * 0.8m; 
    }

    public void RecalculateWageBill()
    {
        if (Squad == null) return;
        
        decimal totalWage = 0;
        foreach (var player in Squad.Players)
        {
            // Fórmula Simples de Salário: (Overall^2) * 10
            // Ex: Overall 80 -> 6400 * 10 = 64.000 mensais
            // Ex: Overall 90 -> 8100 * 10 = 81.000 mensais
            // Ajuste para ficar realista com as receitas
            totalWage += (decimal)(Math.Pow(player.OverallRating, 2) * 25);
        }
        MonthlyWageBill = totalWage;
    }


    public void EvaluateMatch(MatchState match, bool isHome, bool isBigMatch = false)
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
        
        // --- BÔNUS/PENALIDADE POR JOGO GRANDE ---
        if (isBigMatch)
        {
            if (resultScore > 0) 
            {
                resultScore += 2; // Vitória vale muito mais (Total 5)
                // Receita Extra na próxima rodada (simulada via flag ou evento)
                EventLog.Add("VITÓRIA ÉPICA: Torcida em êxtase com triunfo em jogo grande!");
            }
            else if (resultScore < 0)
            {
                resultScore -= 2; // Derrota pesa muito mais (Total -5)
                EventLog.Add("DECEPÇÃO: Derrota em jogo grande abala as estruturas!");
            }
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
        // Atualizar Coach Specialty
        myTeam.Coach.UpdateSpecialty(coherent);
        
        // --- 4. Impacto da FILOSOFIA TÁTICA ---
        // Se Fit < 50 e perdeu: Instabilidade +2
        // Se Fit > 80 e ganhou: Moral +3 (Extra)
        
        if (myTeam.TacticalFit < 50 && resultScore < 0)
        {
            TeamInstability += 2;
        }
        else if (myTeam.TacticalFit > 80 && resultScore > 0)
        {
            // Bônus moral para o time todo
             myTeam.Players.ForEach(p => p.UpdateMorale(3));
        }

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
        // Sincronizar com o Time (para afetar a engine)
        myTeam.Instability = TeamInstability;
        myTeam.CoachPressure = CoachPressure;
        
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

        // --- SISTEMA DE CRISE ORGÂNICA E PRESSÃO ---
        EvaluateLockerRoomDynamics();
        ApplyCrisisLogic(resultScore);
        EvaluateLockerRoomDynamics();
        ApplyCrisisLogic(resultScore);
        UpdateCoachPressure(resultScore, isBigMatch); 
    }

    private void EvaluateLockerRoomDynamics()
    {
        // Analisa a hierarquia do elenco e aplica efeitos
        var leaders = Squad.Players.Where(p => p.LockerRoomInfluence > 70).ToList();
        var unhappyLeaders = leaders.Where(p => p.Morale < 40).ToList();
        var happyLeaders = leaders.Where(p => p.Morale > 75).ToList();

        // 1. Efeito Negativo dos Líderes Insatisfeitos
        foreach (var leader in unhappyLeaders)
        {
            TeamInstability += 3;
            // Chance de Evento Individual (20% se moral muito baixa)
            if (leader.Morale < 35 && new Random().NextDouble() < 0.20)
            {
                EventLog.Add($"VESTIÁRIO: {leader.Name} (Líder) questiona o ambiente publicamente.");
            }
        }

        // 2. Efeito Positivo dos Líderes Satisfeitos
        foreach (var leader in happyLeaders)
        {
             TeamInstability -= 2;
             BoardTrust += 0.5; // Ajuda um pouco na confiança
        }

        // 3. Conflito Interno / Racha (Se houver 2+ líderes insatisfeitos)
        if (unhappyLeaders.Count >= 2)
        {
             if (new Random().NextDouble() < 0.15) // 15%
             {
                 TeamInstability += 5;
                 CoachPressure += 3;
                 EventLog.Add("RACHA NO ELENCO: Líderes insatisfeitos dividem o vestiário!");
             }
        }
    }

    private void ApplyCrisisLogic(double resultScore)
    {
        // 1. Rastrear Últimos 5 Jogos
        int points = 0;
        if (resultScore >= 3) points = 3;
        else if (resultScore >= 1) points = 1;
        
        RecentPoints.Add(points);
        if (RecentPoints.Count > 5) RecentPoints.RemoveAt(0);

        int recentSum = RecentPoints.Sum();

        // 2. Amplificação de Derrota em Crise
        bool isDefeat = resultScore < 0; // -3 no EvaluateMatch representa derrota
        
        if (TeamInstability > 80 && isDefeat)
        {
            BoardTrust -= 4;
            TeamInstability += 3;
            EventLog.Add("A crise se agrava após nova derrota!");
        }

        // 3. Calcular Risco de Crise
        CrisisRisk = 0;
        if (TeamInstability > 80) CrisisRisk++;
        if (recentSum <= 4) CrisisRisk++; // 4 pontos ou menos em 15 possíveis
        if (BoardTrust < 40) CrisisRisk++;

        // 4. Resolver Ultimato (Se houver)
        if (CoachUnderUltimatum)
        {
            if (resultScore >= 3) // Vitória
            {
                CoachUnderUltimatum = false;
                BoardTrust += 6;
                TeamInstability -= 8;
                EventLog.Add("RESPOSTA IMEDIATA: O time vence e salva o emprego do técnico!");
            }
            else if (resultScore >= 0) // Empate (resultScore é 1)
            {
                BoardTrust -= 2;
                EventLog.Add("TENSÃO CONTINUA: Empate não convence a diretoria.");
            }
            else // Derrota
            {
                // Aqui apenas sinalizamos, a demissão real seria processada externamente ou logada
                EventLog.Add("FIM DA LINHA: Diretoria decide demitir o técnico após ultimato não cumprido.");
                BoardTrust = 0; // Forçar trust 0 para indicar demissão iminente
            }
        }
        else
        {
            // 5. Verificar Novos Eventos de Crise (Apenas se NÃO estiver sob ultimato já)
            CheckForCrisisEvents();

            // 6. Verificar Gatilho de Ultimato
            // Gatilho Básico: Instabilidade > 85 && Trust < 35 && Desempenho Ruim
            
            // Ajuste por Expectativa
            double trustThreshold = 35;
            if (SeasonExpectation >= 4) trustThreshold = 40; // Mais exigente
            if (SeasonExpectation <= 2) trustThreshold = 30; // Mais tolerante

            int last3Points = RecentPoints.Skip(Math.Max(0, RecentPoints.Count - 3)).Sum();

            if (TeamInstability > 85 && BoardTrust < trustThreshold && last3Points <= 1)
            {
                CoachUnderUltimatum = true;
                EventLog.Add("ULTIMATO: A diretoria exige uma vitória no próximo jogo ou haverá mudanças!");
            }
        }
    }

    private void CheckForCrisisEvents()
    {
        // Garante apenas 1 evento por rodada (chamado 1x por EvaluateMatch)
        Random rnd = new Random();
        bool triggerEvent = false;

        if (CrisisRisk >= 3)
        {
            if (rnd.NextDouble() < 0.50) triggerEvent = true; // 50%
        }
        else if (CrisisRisk == 2)
        {
            if (rnd.NextDouble() < 0.25) triggerEvent = true; // 25%
        }

        if (triggerEvent)
        {
            string[] events = new string[]
            {
                "Diretoria questiona publicamente o comando técnico.",
                "Grupo de jogadores perde confiança no treinador.",
                "Capitão exige reunião de emergência com a presidência.",
                "Imprensa local crava demissão iminente em caso de novo tropeço."
            };
            
            string selectedEvent = events[rnd.Next(events.Length)];

            EventLog.Add($"CRISE: {selectedEvent}");
            
            // Impacto do evento
            BoardTrust -= 2;
            TeamInstability += 2;
        }
        
        // EVENTO TÁTICO: Questão da Filosofia
        // Se 3 ou mais derrotas seguidas E TacticalFit < 60
        bool threeLosses = RecentPoints.Count >= 3 && !RecentPoints.TakeLast(3).Any(p => p > 0);
        
        if (threeLosses && Squad.TacticalFit < 60 && rnd.NextDouble() < 0.20)
        {
            EventLog.Add("VESTIÁRIO: Elenco questiona se o estilo de jogo do técnico é adequado.");
            CoachPressure += 3;
            TeamInstability += 2;
        }
    }
    
    private void UpdateCoachPressure(double resultScore, bool isImportantMatch)
    {
        // 1. Atualização Base por Resultado
        if (resultScore >= 3) CoachPressure -= 4;       // Vitória
        else if (resultScore == 1) CoachPressure += 1;  // Empate
        else CoachPressure += 5;                        // Derrota

        // 2. Amplificadores
        if (TeamInstability > 70) CoachPressure += 2;
        if (BoardTrust < 40) CoachPressure += 3;
        
        
        // Ajuste por Expectativa (Se time forte perdeu, pressão aumenta mais)
        bool isDefeat = resultScore < 0;
        if (SeasonExpectation >= 4 && isDefeat)
        {
             CoachPressure += 1; // Extra penalty
        }
        
        // Sequência sem vitórias >= 3 (usando RecentPoints)
        int last3Points = RecentPoints.Skip(Math.Max(0, RecentPoints.Count - 3)).Sum();
        bool winlessStreak3 = RecentPoints.Count >= 3 && !RecentPoints.TakeLast(3).Any(p => p == 3);
        
        if (winlessStreak3) CoachPressure += 4;

        // 3. Recuperação Natural
        // 2 Vitórias seguidas
        bool twoWinsStreak = RecentPoints.Count >= 2 && RecentPoints.Skip(Math.Max(0, RecentPoints.Count - 2)).All(p => p == 3);
        if (twoWinsStreak) CoachPressure -= 6;

        // Vitória Importante (Simplificação: se ganhou e era vs G4 ou Título)
        // Como não temos acesso fácil ao oponente aqui sem refatorar muito, 
        // vamos usar uma flag passada ou assumir que vitórias com placar elastico contam como importantes por enquanto.
        // Ou melhor, assumir isImportantMatch passada pelo caller se possível.
        // Por ora, vamos simplificar: Vitória de goleada (> 3 gols de diferença) reduz pressão extra.
        // No EvaluateMatch já calculamos resultScore. Se resultScore >= 4 (Vitória + Goleada)
        // Por ora, vamos simplificar: Vitória de goleada (> 3 gols de diferença) reduz pressão extra.
        // No EvaluateMatch já calculamos resultScore. Se resultScore >= 4 (Vitória + Goleada)
        if (resultScore >= 4) CoachPressure -= 8;
        
        // Vitória em Jogo Grande (ImportantMatch)
        if (isImportantMatch && resultScore > 0)
        {
             CoachPressure -= 6; // Alívio extra
        }

        // 4. Clamp Final
        if (CoachPressure < 0) CoachPressure = 0;
        if (CoachPressure > 100) CoachPressure = 100;
        
        // 5. Efeitos / Eventos de Pressão
        Random rnd = new Random();
        if (CoachPressure > 85)
        {
            if (rnd.NextDouble() < 0.20) // 20%
            {
                EventLog.Add("MÍDIA: A imprensa questiona duramente as escolhas do treinador!");
            }
        }
        
        if (CoachPressure > 95)
        {
             EventLog.Add("BASTIDORES: Diretoria considera demissão iminente devido à pressão insustentável.");
        }
    }

    public void EvaluateExpectation(int currentPosition)
    {
        // Comparar Posição vs Expectativa
        // Exp 5 (Título): Espera 1º a 3º
        // Exp 4 (G4): Espera 1º a 5º 
        // Exp 2 (Meio): Espera < 12º
        // Exp 1 (Z4): Espera > Z4 (vamos assumir 17-20 como Z4 em liga de 20, mas aqui temos 4 times... adaptando)
        
        // Como a liga é pequena (4 times), vamos adaptar as faixas:
        // 5 (Título) -> Espera 1º
        // 4 (G4) -> Espera top 2
        // 2 (Meio) -> Espera não ser último
        // 1 (Rebaixamento) -> Aceita qualquer coisa

        bool isBelowExpec = false;
        bool isAboveExpec = false;

        // Lógica Adaptada para Liga de 4 Times
        if (SeasonExpectation == 5) // Título
        {
            if (currentPosition > 1) isBelowExpec = true;
        }
        else if (SeasonExpectation == 4) // G4 (Top 2 aqui)
        {
            if (currentPosition > 2) isBelowExpec = true;
             else if (currentPosition == 1) isAboveExpec = true;
        }
        else if (SeasonExpectation >= 2) // Meio
        {
            if (currentPosition == 4) isBelowExpec = true; // Último é ruim
            else if (currentPosition <= 2) isAboveExpec = true;
        }
        
        // Aplicar Efeitos
        if (isBelowExpec)
        {
            ExpectationStatus = "Abaixo do esperado";
            BoardTrust -= 1;
            CoachPressure += 2;
            
            // Evento de Frustração (20%)
            if (new Random().NextDouble() < 0.20)
                EventLog.Add("DIRETORIA: O desempenho na tabela está abaixo do esperado para a temporada.");
        }
        else if (isAboveExpec)
        {
            ExpectationStatus = "Acima do esperado";
            BoardTrust += 1;
        }
        else
        {
             ExpectationStatus = "Dentro do esperado";
        }

        // Clamp Trust e Pressure
        if (BoardTrust < 0) BoardTrust = 0; if (BoardTrust > 100) BoardTrust = 100;
        if (CoachPressure < 0) CoachPressure = 0; if (CoachPressure > 100) CoachPressure = 100;
    }


    // --- MÉTODOS FINANCEIROS ---

    public void ProcessMatchRevenue(MatchState matchResult)
    {
        // Receita de Bilheteria (Apenas em casa)
        // Ocupação Base: 70%
        double occupancyRate = 0.70;

        // Bônus por Expectativa/Momento
        if (SeasonExpectation >= 4) occupancyRate += 0.10; // Torcida fiel
        if (UnbeatenStreak > 3) occupancyRate += 0.10;     // Empolgação
        if (TeamInstability > 50) occupancyRate -= 0.15;   // Protesto/Medo
        if (CrisisRisk > 0) occupancyRate -= 0.10;

        // Clamp Ocupação
        if (occupancyRate > 1.0) occupancyRate = 1.0;
        if (occupancyRate < 0.1) occupancyRate = 0.1;

        int attendance = (int)(StadiumCapacity * occupancyRate);
        decimal revenue = attendance * TicketPrice;

        Balance += revenue;
        LastMatchRevenue = revenue;

        // Custos Operacionais do Jogo (20% da receita)
        Balance -= (revenue * 0.20m);
    }

    public void ProcessMonthlyFinancials()
    {
        // 1. Receitas Fixas
        Balance += MonthlySponsorship;

        // 2. Despesas Fixas (Salários)
        RecalculateWageBill(); // Garante atualizado
        Balance -= MonthlyWageBill;

        // 3. Juros de Dívida
        if (Balance < 0)
        {
            decimal interest = Math.Abs(Balance) * 0.05m; // 5% de juros
            Balance -= interest;
            
            // Penalidade por Dívida
            BoardTrust -= 2;
            CoachPressure += 2;
            EventLog.Add($"FINANÇAS: Dívida gera juros de {interest:C0} e irrita a diretoria.");
        }

        // 4. Atualizar Status
        if (Balance < 0) FinancialStatus = "ENDIVIDADO";
        else if (Balance < MonthlyWageBill * 2) FinancialStatus = "Crítico";
        else if (Balance < MonthlyWageBill * 6) FinancialStatus = "Apertado";
        else FinancialStatus = "Estável";
    }

    public void ProcessTransferMarket(string currentLeagueStatus) // "Normal", "Leader", "Champion"
    {
        if (Squad == null) return;
        
        // 1. Atualizar Valores e Desejos de Todos
        foreach (var player in Squad.Players)
        {
            player.CalculateMarketValue(currentLeagueStatus);
            player.UpdateStayDesire(
                TeamInstability, 
                CoachPressure, 
                ExpectationStatus, 
                Squad.StartingEleven.Contains(player)
            );
        }
        
        // 2. Verificar Pedidos de Transferência
        // Se Influente e Desire < 35 -> 25% chance de pedir pra sair
        var unhappyInfluencers = Squad.Players.Where(p => p.LockerRoomInfluence > 60 && p.StayDesire < 35).ToList();
        
        foreach (var p in unhappyInfluencers)
        {
            if (new Random().NextDouble() < 0.25)
            {
                EventLog.Add($"MERCADO: {p.Name} está infeliz e solicitou transferência! (Desejo: {p.StayDesire})");
                TeamInstability += 3;
                BoardTrust -= 1;
            }
        }
        
        // 3. Venda Forçada (Se caixa negativo)
        if (Balance < 0)
        {
            // Chance de 40% se endividado
            if (new Random().NextDouble() < 0.40)
            {
                // Vender o jogador mais valioso
                var mostValuable = Squad.Players.OrderByDescending(p => p.MarketValue).FirstOrDefault();
                if (mostValuable != null)
                {
                    // Simular Venda
                    decimal saleValue = mostValuable.MarketValue * 0.9m; // Vende rápido, um pouco abaixo
                    Balance += saleValue;
                    Squad.Players.Remove(mostValuable);
                    Squad.StartingEleven.Remove(mostValuable); // Se titular
                    Squad.Bench.Remove(mostValuable);
                    
                    // Repor se necessário (simplificado: gera um genérico da base)
                    var replacement = new Player($"Base {mostValuable.Position}", mostValuable.Position, 70);
                    Squad.AddPlayer(replacement);
                    if (Squad.StartingEleven.Count < 11) Squad.SetStartingEleven(Squad.Players.Take(11).ToList());
                    
                    EventLog.Add($"CRISE FINANCEIRA: Diretoria vendeu {mostValuable.Name} por {saleValue:C0} para cobrir dívidas.");
                    TeamInstability += 5;
                    CoachPressure += 5;
                    BoardTrust -= 5;
                }
            }
        }
    }

}
