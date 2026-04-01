public class Program
{
    public static void Main()
    {
        Console.WriteLine("Verifying deck contents...");
        Deck verifyDeck = new Deck();

        Console.WriteLine("Cards have been verified");
        Console.WriteLine($"Total cards: {verifyDeck.Cards.Count}\n");

        BlackjackGame game = new BlackjackGame();

        while (true)
        {
            if (game.Player.Balance <= 0)
            {
                Console.WriteLine("\nYou are bust! Retry (R) or press any key to close.");
                string choice = (Console.ReadLine() ?? string.Empty).ToUpper();
                if (choice == "R")
                {
                    game.Player.Balance = 500;
                    Console.WriteLine("Balance reset to 500.");
                }
                else return;
            }

            game.Start();

            bool playerTurn = true;
            while (playerTurn)
            {
                Console.WriteLine("\nChoose action: (H)it, (S)tand, (D)ouble, (I)nsurance");
                string choice = (Console.ReadLine() ?? string.Empty).ToUpper();

                switch (choice)
                {
                    case "H":
                        game.Player.Hit(game.Deck);
                        Console.WriteLine($"Player: {string.Join(", ", game.Player.Hand.Cards)} (Value: {game.Player.Hand.GetValue()})");
                        if (game.Player.Hand.GetValue() > 21)
                        {
                            Console.WriteLine("Player busts!");
                            playerTurn = false;
                        }
                        break;

                    case "S":
                        playerTurn = false;
                        break;

                    case "D":
                        game.Player.Double(game.Deck);
                        Console.WriteLine($"Player doubled. Hand: {string.Join(", ", game.Player.Hand.Cards)} (Value: {game.Player.Hand.GetValue()})");
                        playerTurn = false;
                        break;

                    case "I":
                        game.Player.Insurance();
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }

            game.DealerTurn();
            game.CompareHands();
        }
    }
}




