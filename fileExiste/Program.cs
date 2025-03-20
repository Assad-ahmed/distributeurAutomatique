using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static string ticketFile = Path.Combine(Path.GetTempPath(), "fnumero.txt");
    static Dictionary<string, int> ticketNumbers = new Dictionary<string, int>();
    static Dictionary<string, string> assignedAccounts = new Dictionary<string, string>();
    static List<string> clients = new List<string>();

    // Message standard avec des espaces réservés (placeholders)
    const string TICKET_MESSAGE = "Votre numéro est {0}, et il y a {1} personne(s) en attente avant vous.";

    static void Main(string[] args)
    {
        InitTicketFile();
        bool continuer = true;

        while (continuer)
        {
            Console.WriteLine("\nQuel type d'opération souhaitez-vous effectuer ?");
            Console.WriteLine("1. Versement");
            Console.WriteLine("2. Retrait");
            Console.WriteLine("3. Informations");
            Console.Write("Votre choix : ");
            string choice = Console.ReadLine();

            if (!new[] { "1", "2", "3" }.Contains(choice))
            {
                Console.WriteLine("❌ Choix invalide. Veuillez réessayer.");
                continue;
            }

            string accountNumber;
            do
            {
                Console.Write("Entrez votre numéro de compte (5 chiffres) : ");
                accountNumber = Console.ReadLine();

                if (!IsValidAccountNumber(accountNumber))
                {
                    Console.WriteLine("❌ Numéro de compte invalide ou déjà utilisé.");
                    continue;
                }

                break;
            } while (true);

            string firstName;
            do
            {
                Console.Write("Entrez votre prénom : ");
                firstName = Console.ReadLine();

                if (!IsValidName(firstName))
                {
                    Console.WriteLine("❌ Prénom invalide. Il doit comporter au moins 2 lettres et ne contenir que des lettres.");
                    continue;
                }

                break;
            } while (true);

            string lastName;
            do
            {
                Console.Write("Entrez votre nom : ");
                lastName = Console.ReadLine();

                if (!IsValidName(lastName))
                {
                    Console.WriteLine("❌ Nom invalide. Il doit comporter au moins 2 lettres et ne contenir que des lettres.");
                    continue;
                }

                break;
            } while (true);

            GenerateTicket(choice, accountNumber, firstName, lastName);

            Console.Write("Voulez-vous prendre un autre numéro ? (o/n) : ");
            continuer = Console.ReadLine()?.ToLower() == "o";
        }

        Console.WriteLine("\n📋 Liste des clients et leurs numéros de ticket :");
        if (clients.Count > 0)
        {
            clients.ForEach(Console.WriteLine);
        }
        else
        {
            Console.WriteLine("Aucun client enregistré.");
        }
    }

    static void InitTicketFile()
    {
        if (!File.Exists(ticketFile))
        {
            File.WriteAllText(ticketFile, "V=0\nR=0\nI=0\nACCOUNTS=\n");
        }

        foreach (var line in File.ReadAllLines(ticketFile))
        {
            if (line.StartsWith("ACCOUNTS="))
            {
                var accounts = line.Substring(9).Split(',');
                foreach (var acc in accounts)
                {
                    if (!string.IsNullOrWhiteSpace(acc))
                        assignedAccounts[acc] = "";
                }
            }
            else
            {
                var parts = line.Split('=');
                ticketNumbers[parts[0]] = int.Parse(parts[1]);
            }
        }
    }

    static bool IsValidAccountNumber(string accountNumber)
    {
        return accountNumber.Length == 5 &&
               int.TryParse(accountNumber, out _) &&
               !assignedAccounts.ContainsKey(accountNumber);
    }

    static bool IsValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.Length >= 2 &&
               Regex.IsMatch(name, @"^[a-zA-Z]+$");
    }

    static void GenerateTicket(string operationType, string accountNumber, string firstName, string lastName)
    {
        var typeMap = new Dictionary<string, string> { { "1", "V" }, { "2", "R" }, { "3", "I" } };
        string ticketType = typeMap[operationType];

        ticketNumbers[ticketType]++;
        string ticketNumber = $"{ticketType}-{ticketNumbers[ticketType]}";

        assignedAccounts[accountNumber] = ticketNumber;
        clients.Add($"{firstName} {lastName} (Compte : {accountNumber}) - Ticket : {ticketNumber}");

        // Affichage du message dynamique
        string message = string.Format(TICKET_MESSAGE, ticketNumber, ticketNumbers[ticketType] - 1);
        Console.WriteLine($"✅ {message}");

        UpdateTicketFile();
    }

    static void UpdateTicketFile()
    {
        List<string> lines = new List<string>();
        foreach (var entry in ticketNumbers)
        {
            lines.Add($"{entry.Key}={entry.Value}");
        }

        string accountsLine = "ACCOUNTS=" + string.Join(",", assignedAccounts.Keys);
        lines.Add(accountsLine);

        File.WriteAllLines(ticketFile, lines);
    }
}
