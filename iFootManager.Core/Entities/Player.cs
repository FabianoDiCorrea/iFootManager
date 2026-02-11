namespace iFootManager.Core.Entities;

// Representa um jogador de futebol
public class Player
{
    public string Name { get; set; }        // Nome do jogador
    public Position Position { get; set; }  // Posição tática
    public int OverallRating { get; set; }  // Nota geral de habilidade (0-100)

    public Player(string name, Position position, int overallRating)
    {
        Name = name;
        Position = position;
        OverallRating = overallRating;
    }
}
