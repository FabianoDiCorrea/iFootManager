# iFootManager - Núcleo de Simulação

Este é o projeto inicial do **iFootManager**, um simulador de gerenciamento de futebol focado na engine de partida.

## Pré-requisitos

- .NET 8 SDK instalado.

## Como Executar

### Opção 1: Usando o Script (Recomendado no Windows)
Basta clicar duas vezes no arquivo `run_simulation.bat` na raiz do projeto.

### Opção 2: Via Terminal
Abra o terminal na pasta raiz do projeto e execute:

```bash
dotnet run --project iFootManager.Simulator/iFootManager.Simulator.csproj
```

## Estrutura do Projeto

- **iFootManager.Core**: Contém a lógica do jogo (Entidades, Engine, MatchState).
- **iFootManager.Simulator**: Aplicação Console para rodar a simulação e visualizar os resultados.

## Funcionalidades Atuais

- Simulação de partida (90 minutos).
- Eventos de Gol e Defesas.
- Substituições simples.
- Mudanças táticas.
- Textos e comentários 100% em Português (PT-BR).
