using System;
using System.Globalization;
using System.Security.Cryptography;

#pragma warning disable CA1303 // String literals are intentionally used for console text.

static class Program
{
    private const int MaxHealth = 20;
    private static readonly string[] Monsters = { "Goblin", "Skeleton", "Orc", "Cave Spider", "Dark Knight", "Troll" };
    private static readonly string[] FightFleeOptions = { "fight", "flee" };

    // High Score system
    private static int HighScore = 0;

    private enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    static void Main()
    {
        Console.Title = "Dungeon Escape";
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();
        ShowWelcome();

        do
        {
            Difficulty difficulty = AskDifficulty();
            PlayAdventure(difficulty);
            Console.WriteLine();
        }
        while (AskPlayAgain());

        Console.WriteLine("Thanks for playing! Your next adventure awaits.");
    }

    private static void DisplayTitle()
    {
        WriteLineColor(ConsoleColor.Cyan, "╔════════════════════════════════════════════════════╗");
        WriteLineColor(ConsoleColor.Cyan, "║              DUNGEON ESCAPE ADVENTURE              ║");
        WriteLineColor(ConsoleColor.Cyan, "╠════════════════════════════════════════════════════╣");
        WriteLineColor(ConsoleColor.Cyan, "║    Venture into the darkness, defeat your foes,    ║");
        WriteLineColor(ConsoleColor.Cyan, "║      and escape with glory and treasure.           ║");
        WriteLineColor(ConsoleColor.Cyan, "╚════════════════════════════════════════════════════╝");
    }

    private static void ShowWelcome()
    {
        DisplayTitle();
        WriteLineColor(ConsoleColor.Yellow, "Welcome to the dungeon. Survive the encounters, collect treasure, and escape alive.");
        if (HighScore > 0)
        {
            WriteLineColor(ConsoleColor.Green, $"Current High Score: {HighScore}");
        }
        Console.WriteLine();
    }

    private static Difficulty AskDifficulty()
    {
        WriteColor(ConsoleColor.Magenta, "Choose difficulty [easy, normal, hard]: ");
        while (true)
        {
            string? input = Console.ReadLine()?.Trim();
            if (string.Equals(input, "easy", StringComparison.OrdinalIgnoreCase))
                return Difficulty.Easy;
            if (string.Equals(input, "normal", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "medium", StringComparison.OrdinalIgnoreCase))
                return Difficulty.Normal;
            if (string.Equals(input, "hard", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "difficult", StringComparison.OrdinalIgnoreCase))
                return Difficulty.Hard;

            WriteColor(ConsoleColor.Red, "Enter easy, normal, or hard: ");
        }
    }

    private static bool AskPlayAgain()
    {
        Console.Write("Play again? (y/n): ");
        bool result = ReadYesNo();
        if (result)
        {
            Console.Clear();
            ShowWelcome();
        }

        return result;
    }

    private static void PlayAdventure(Difficulty difficulty)
    {
        int health = MaxHealth;
        int gold = 0;
        int rooms = difficulty switch
        {
            Difficulty.Easy => GetRandom(4, 8),
            Difficulty.Normal => GetRandom(4, 7),
            Difficulty.Hard => GetRandom(3, 7),
            _ => GetRandom(4, 7)
        };
        bool hasMagicWeapon = false;

        WriteLineColor(ConsoleColor.Green, $"Difficulty selected: {difficulty}");
        WriteLineColor(ConsoleColor.Green, "Your journey begins...");
        Console.WriteLine();

        for (int room = 1; room <= rooms; room++)
        {
            Console.WriteLine($"--- Room {room} ---");
            int eventType = GetRandom(0, 6);

            switch (eventType)
            {
                case 0:
                    gold += FindTreasure(difficulty);
                    break;
                case 1:
                    health -= TriggerTrap(difficulty);
                    break;
                case 2:
                    health = EncounterMonster(health, ref gold, hasMagicWeapon, difficulty);
                    break;
                case 3:
                    health = DiscoverPotion(health, difficulty);
                    break;
                case 4:
                    hasMagicWeapon = DiscoverMagicWeapon(ref gold, hasMagicWeapon, difficulty);
                    break;
                default:
                    health = DiscoverHiddenFountain(health, difficulty);
                    break;
            }

            if (health <= 0)
            {
                Console.WriteLine("You collapse from your wounds. The dungeon claims another explorer.");
                break;
            }

            Console.WriteLine($"Health: {health}   Gold: {gold}   Weapon: {(hasMagicWeapon ? "Enchanted" : "Basic")}");
            Console.WriteLine();
        }

        ShowAdventureSummary(health, gold, hasMagicWeapon);
    }

    private static void ShowAdventureSummary(int health, int gold, bool hasMagicWeapon)
    {
        Console.WriteLine("=== Adventure Complete ===");

        int score = gold + health * 2 + (hasMagicWeapon ? 10 : 0);
        if (score > HighScore)
        {
            HighScore = score;
            WriteLineColor(ConsoleColor.Yellow, "New High Score!");
        }

        if (health > 0)
        {
            Console.WriteLine("You escaped the dungeon! Your bravery is rewarded.");
            Console.WriteLine($"Final score: {score}");
            Console.WriteLine($"Ending health: {health}");
            Console.WriteLine($"Total gold: {gold}");
            Console.WriteLine($"Weapon status: {(hasMagicWeapon ? "Enchanted" : "Basic")}");
        }
        else
        {
            Console.WriteLine("Your adventure ends here. Try again to beat the dungeon.");
            Console.WriteLine($"Gold collected: {gold}");
        }

        WriteLineColor(ConsoleColor.Cyan, $"High Score: {HighScore}");
    }

    private static int FindTreasure(Difficulty difficulty)
    {
        int modifier = difficulty == Difficulty.Easy ? 3 : difficulty == Difficulty.Hard ? -1 : 0;
        int coins = Math.Max(1, GetRandom(5, 21) + modifier);
        WriteLineColor(ConsoleColor.Yellow, "You find a hidden chest!");
        WriteLineColor(ConsoleColor.Yellow, $"You collect {coins} gold coins.");
        return coins;
    }

    private static int TriggerTrap(Difficulty difficulty)
    {
        int modifier = difficulty == Difficulty.Hard ? 2 : difficulty == Difficulty.Easy ? -1 : 0;
        int damage = Math.Max(1, GetRandom(4, 11) + modifier);
        WriteLineColor(ConsoleColor.Red, "A trap is triggered! Sharp spikes pop up from the floor.");
        WriteLineColor(ConsoleColor.Red, $"You take {damage} damage.");
        return damage;
    }

    private static int DiscoverPotion(int health, Difficulty difficulty)
    {
        int modifier = difficulty == Difficulty.Easy ? 2 : difficulty == Difficulty.Hard ? -2 : 0;
        int heal = Math.Max(1, GetRandom(4, 11) + modifier);
        health = Math.Min(MaxHealth, health + heal);
        Console.WriteLine("You discover a glowing potion in an alcove.");
        Console.WriteLine($"You restore {heal} health.");
        return health;
    }

    private static bool DiscoverMagicWeapon(ref int gold, bool hasMagicWeapon, Difficulty difficulty)
    {
        int bonus = difficulty == Difficulty.Easy ? 2 : difficulty == Difficulty.Hard ? -3 : 0;

        if (hasMagicWeapon)
        {
            Console.WriteLine("You find another enchanted dagger, but your current weapon is already powerful.");
            Console.WriteLine("You sell it for 10 gold.");
            gold += Math.Max(1, 10 + bonus);
            return true;
        }

        Console.WriteLine("You uncover a shining enchanted sword in a forgotten armory.");
        Console.WriteLine("Your attacks will now be stronger.");
        gold += Math.Max(1, 15 + bonus);
        return true;
    }

    private static int DiscoverHiddenFountain(int health, Difficulty difficulty)
    {
        int modifier = difficulty == Difficulty.Easy ? 2 : difficulty == Difficulty.Hard ? -2 : 0;
        int heal = Math.Max(1, GetRandom(5, 11) + modifier);
        health = Math.Min(MaxHealth, health + heal);
        Console.WriteLine("You discover a hidden fountain bubbling with restorative water.");
        Console.WriteLine($"The water restores {heal} health.");
        return health;
    }

    private static int EncounterMonster(int health, ref int gold, bool hasMagicWeapon, Difficulty difficulty)
    {
        string monster = Monsters[GetRandom(0, Monsters.Length)];
        int monsterHealthModifier = difficulty == Difficulty.Hard ? 3 : difficulty == Difficulty.Easy ? -2 : 0;
        int monsterAttackModifier = difficulty == Difficulty.Hard ? 2 : difficulty == Difficulty.Easy ? -1 : 0;
        int rewardModifier = difficulty == Difficulty.Easy ? 3 : difficulty == Difficulty.Hard ? -2 : 0;

        int monsterHealth = Math.Max(1, GetRandom(10, 18) + monsterHealthModifier);
        int monsterAttack = Math.Max(1, GetRandom(3, 8) + monsterAttackModifier);

        Console.WriteLine($"A {monster} appears! It has {monsterHealth} health and attacks fiercely.");
        Console.Write("Do you fight or flee? (fight/flee): ");
        string choice = ReadChoice(FightFleeOptions);

        if (choice == "flee")
        {
            int fleeSuccess = GetRandom(0, 2);
            if (fleeSuccess == 1)
            {
                Console.WriteLine("You manage to escape without taking damage.");
                return health;
            }

            int damage = monsterAttack;
            health -= damage;
            Console.WriteLine($"You fail to escape and take {damage} damage while running.");
            return health;
        }

        while (monsterHealth > 0 && health > 0)
        {
            int playerDamage = GetRandom(4, 10) + (hasMagicWeapon ? 3 : 0);
            monsterHealth -= playerDamage;
            Console.WriteLine($"You strike the {monster} for {playerDamage} damage.");

            if (monsterHealth <= 0)
            {
                int reward = Math.Max(1, GetRandom(10, 26) + (hasMagicWeapon ? 5 : 0) + rewardModifier);
                gold += reward;
                Console.WriteLine($"The {monster} falls! You collect {reward} gold.");
                return health;
            }

            int damage = GetRandom(1, monsterAttack + 1);
            health -= damage;
            Console.WriteLine($"The {monster} hits you for {damage} damage.");
        }

        return health;
    }

    private static int GetRandom(int minValue, int maxValueExclusive)
        => RandomNumberGenerator.GetInt32(minValue, maxValueExclusive);

    private static void WriteColor(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    private static void WriteLineColor(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private static string ReadChoice(string[] validOptions)
    {
        while (true)
        {
            string? input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                continue;
            }

            foreach (string option in validOptions)
            {
                if (string.Equals(input, option, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(input, option[0].ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            Console.Write($"Please type {string.Join(" or ", validOptions)}: ");
        }
    }

    private static bool ReadYesNo()
    {
        while (true)
        {
            string? input = Console.ReadLine()?.Trim();
            if (input is null)
            {
                continue;
            }

            if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "no", StringComparison.OrdinalIgnoreCase)) return false;

            Console.Write("Enter 'y' or 'n': ");
        }
    }
}
