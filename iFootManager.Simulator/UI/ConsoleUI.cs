using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace iFootManager.Simulator.UI
{
    public static class ConsoleUI
    {
        // --- CORES ---
        public static class Colors
        {
            public static ConsoleColor Primary = ConsoleColor.Cyan;
            public static ConsoleColor Success = ConsoleColor.Green;
            public static ConsoleColor Danger = ConsoleColor.Red;
            public static ConsoleColor Warning = ConsoleColor.Yellow;
            public static ConsoleColor Default = ConsoleColor.Gray;
            public static ConsoleColor Muted = ConsoleColor.DarkGray;
            public static ConsoleColor Highlight = ConsoleColor.White;
            public static ConsoleColor Gold = ConsoleColor.DarkYellow;
        }

        // --- ÍCONES (Unicode Flat) ---
        public static class Icons
        {
            public const string Money = "💰";
            public const string Chart = "📈";
            public const string User = "👤";
            public const string Coach = "👔";
            public const string Ball = "⚽";
            public const string Star = "⭐";
            public const string Fire = "🔥";
            public const string Skull = "💀";
            public const string Lock = "🔒";
            public const string Trophy = "🏆";
            public const string Success = "✔️";
            public const string Alert = "⚠️";
            public const string Shield = "🛡️";
            public const string Lightning = "⚡";
            public const string Heart = "❤️";
            public const string BrokenHeart = "💔";
            public const string Whistle = "📣";
        }

        // --- ESTRUTURAS VISUAIS ---

        public static void DrawHeader(string title, string subtitle = "")
        {
            Console.Clear();
            DrawBigTitle("iFootManager");
            
            Console.ForegroundColor = Colors.Primary;
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║ {title.PadRight(76)} ║");
            if (!string.IsNullOrEmpty(subtitle))
            {
                Console.WriteLine($"║ {subtitle.PadRight(76)} ║");
            }
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void DrawBigTitle(string text)
        {
            // ASCII Art Mini
            Console.ForegroundColor = Colors.Highlight;
            Console.WriteLine(@"
   _  _____           _   __  __                                   
  (_)|  ___|__   ___ | |_|  \/  | __ _ _ __   __ _  __ _  ___ _ __ 
  | || |_ / _ \ / _ \| __| |\/| |/ _` | '_ \ / _` |/ _` |/ _ \ '__|
  | ||  _| (_) | (_) | |_| |  | | (_| | | | | (_| | (_| |  __/ |   
  |_||_|  \___/ \___/ \__|_|  |_|\__,_|_| |_|\__,_|\__, |\___|_|   
                                                   |___/           
");
            Console.ResetColor();
        }

        public enum CardStyle { Default, Modern, Bold, Double }

        public static void DrawCard(string title, List<string> lines, ConsoleColor color = ConsoleColor.Gray, CardStyle style = CardStyle.Modern)
        {
            Console.ForegroundColor = color;

            string tl, tr, bl, br, h, v;
            
            switch (style)
            {
                case CardStyle.Bold:   tl="┏"; tr="┓"; bl="┗"; br="┛"; h="━"; v="┃"; break;
                case CardStyle.Double: tl="╔"; tr="╗"; bl="╚"; br="╝"; h="═"; v="║"; break;
                case CardStyle.Modern: tl="╭"; tr="╮"; bl="╰"; br="╯"; h="─"; v="│"; break;
                default:               tl="┌"; tr="┐"; bl="└"; br="┘"; h="─"; v="│"; break;
            }

            // Título estilizado
            string titleBar = $"{tl}{h} {title.ToUpper()} ";
            string trail = new string(h[0], 76 - titleBar.Length) + tr;
            
            Console.WriteLine(titleBar + trail);
            
            foreach (var line in lines)
            {
                string content = line.Length > 74 ? line.Substring(0, 71) + "..." : line;
                Console.WriteLine($"{v} {content.PadRight(74)} {v}");
            }
            
            Console.WriteLine(bl + new string(h[0], 76) + br);
            Console.ResetColor();
        }

        public static void DrawProgressBar(string label, double current, double max, ConsoleColor color, bool animate = false)
        {
            int barWidth = 25;
            double percent = Math.Clamp(current / max, 0, 1);
            int filled = (int)(percent * barWidth);
            
            Console.Write($"{label.PadRight(15)} ");
            
            if (animate)
            {
                // Simulação de animação (prechendo)
                Console.ForegroundColor = color;
                Console.Write("[");
                for(int i=0; i<filled; i++) 
                { 
                    Console.Write("█"); 
                    Thread.Sleep(20); // Delay visual rápido
                }
                Console.Write(new string('░', barWidth - filled));
                Console.Write($"] {current:F0}/{max:F0}");
            }
            else
            {
                string bar = new string('█', filled) + new string('░', barWidth - filled);
                Console.ForegroundColor = color;
                Console.Write($"[{bar}] {current:F0}/{max:F0}");
            }
            
            Console.ResetColor();
            Console.WriteLine();
        }

        public static string FormatCurrency(decimal value)
        {
            return value.ToString("C0");
        }

        public static string GetBadge(string type)
        {
            switch (type)
            {
                case "Leader": return $"{Icons.Star} LÍDER";
                case "Crisis": return $"{Icons.Skull} CRISE";
                case "BigMatch": return $"{Icons.Fire} JOGO GRANDE";
                case "Champion": return $"{Icons.Trophy} CAMPEÃO";
                case "LastChance": return $"{Icons.Lightning} ÚLTIMA CHANCE";
                case "Stable": return $"{Icons.Shield} CLUBE ESTÁVEL";
                default: return "";
            }
        }

        // --- AVATAR SYSTEM ---
        public static void DrawAvatar(string name, string role, string feeling = "Neutral")
        {
            // Avatar Procedural Simples
            string face = "( . . )";
            if (feeling == "Happy") face = "( ^_^ )";
            if (feeling == "Angry") face = "( ò_ó )";
            if (feeling == "Sad")   face = "( T_T )";
            if (feeling == "Dead")  face = "( x_x )";

            var lines = new List<string>
            {
                "   _____   ",
                $"  /{face}\\  ",
                "  |  |  |  ",
                $"  {role.PadRight(7)}  "
            };

            // Desenhar lado a lado com informações passadas seria ideal, 
            // mas por simplificação vamos desenhar um mini card.
            
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"   _____        {name}");
            Console.WriteLine($"  /{face}\\       {role}");
            Console.WriteLine($"  |  |  |  ");
            Console.ResetColor();
        }
        
        // --- COMPATIBILIDADE ---
        // Helpers antigos para não quebrar código existente, redirecionando para novas classes
        public static ConsoleColor ColorPrimary => Colors.Primary;
        public static ConsoleColor ColorSuccess => Colors.Success;
        public static ConsoleColor ColorDanger => Colors.Danger;
        public static ConsoleColor ColorWarning => Colors.Warning;
        public static ConsoleColor ColorDefault => Colors.Default;
        public static ConsoleColor ColorMuted => Colors.Muted;

        public const string IconMoney = Icons.Money;
        public const string IconCoach = Icons.Coach;
        public const string IconStar = Icons.Star;
        public const string IconSkull = Icons.Skull;
        public const string IconSuccess = Icons.Success;
    }
}
