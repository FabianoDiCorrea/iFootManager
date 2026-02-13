using iFootManager.Core;

namespace iFootManager.Core.Entities;

// Representa o técnico do time
public class Coach
{
    public string Name { get; private set; }
    public TacticalPosture PreferredStyle { get; private set; } // Legado/Instrução
    public TacticalStyle Style { get; private set; } // Filosofia Macro
    
    // Career Properties
    public CoachSpecialty PrimarySpecialty { get; private set; }
    public double PrimarySpecialtyStrength { get; private set; } = 100;
    
    public CoachSpecialty? SecondarySpecialty { get; private set; }
    public double SecondarySpecialtyStrength { get; private set; } = 100;
    
    public string? Weakness { get; private set; }
    public double WeaknessStrength { get; private set; } = 100;

    public Coach(string name, TacticalPosture preferredStyle, CoachSpecialty primarySpecialty)
    {
        Name = name;
        PreferredStyle = preferredStyle;
        PrimarySpecialty = primarySpecialty;
        
        // Inicializa Filosofia com base no Estilo Preferido (Simplificado)
        // Ou aleatório
        var styles = Enum.GetValues(typeof(TacticalStyle));
        Style = (TacticalStyle)styles.GetValue(new Random().Next(styles.Length));
        
        // Correção de coerência básica
        if (PreferredStyle == TacticalPosture.VeryDefensive) Style = TacticalStyle.Defensive;
        if (PreferredStyle == TacticalPosture.AllOutAttack) Style = TacticalStyle.HighPress;
    }
    
    // Construtor legado para compatibilidade (se necessário), mas idealmente deve ser removido ou atualizado
    public Coach(string name, TacticalPosture preferredStyle) : this(name, preferredStyle, CoachSpecialty.Motivator)
    {
    }

    public void UpdateSpecialty(bool isCoherent)
    {
        double change = 0;
        if (isCoherent)
        {
            // +2 a +4
            change = new Random().Next(2, 5); 
        }
        else
        {
            // -3 a -6
            change = -new Random().Next(3, 7);
        }
        
        PrimarySpecialtyStrength += change;
        
        // Clamp 0 - 110
        if (PrimarySpecialtyStrength < 0) PrimarySpecialtyStrength = 0;
        if (PrimarySpecialtyStrength > 110) PrimarySpecialtyStrength = 110;
    }
}
