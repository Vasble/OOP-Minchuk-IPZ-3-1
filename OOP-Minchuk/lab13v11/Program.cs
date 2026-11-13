using System.Text;
using lab19v11.Common;
using lab19v11.Exceptions;
using lab19v11.Models;
using lab19v11.Services;

namespace lab19v11;

internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var paymentService = new PaymentService("LiqPay");

        DemoProcessPayments(paymentService);
        DemoRefunds(paymentService);
        PrintTransactions(paymentService);
    }

    private static void DemoProcessPayments(PaymentService service)
    {
        PrintHeader("1. ProcessPayment — обробка PaymentGatewayException через try-catch");

        TryPay(service, 1_250.00m);
        TryPay(service, 499.99m);
        TryPay(service, 75_000.00m);   
        TryPay(service, -100.00m); 

        Console.WriteLine("\n  >> Імітуємо збій шлюзу (IsGatewayAvailable = false)");
        service.IsGatewayAvailable = false;
        TryPay(service, 300.00m);    

        service.IsGatewayAvailable = true;
        Console.WriteLine("  >> Шлюз знову доступний");
        TryPay(service, 3_400.00m);
    }

    private static void TryPay(PaymentService service, decimal amount)
    {
        Console.Write($"\n  Платіж {amount,10:F2} грн: ");

        try
        {
            service.ProcessPayment(amount);
            WriteColored("успішно проведено.", ConsoleColor.Green);
        }
        catch (PaymentGatewayException ex)
        {
            WriteColored("помилка шлюзу!", ConsoleColor.Red);
            Console.WriteLine($"    Шлюз:       {ex.GatewayName}");
            Console.WriteLine($"    Код:        {ex.ErrorCode}");
            Console.WriteLine($"    Повідомл.:  {ex.Message}");
            if (ex.InnerException is not null)
                Console.WriteLine($"    Причина:    {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            WriteColored("некоректні дані!", ConsoleColor.Yellow);
            Console.WriteLine($"    {ex.Message}");
        }
    }

    private static void DemoRefunds(PaymentService service)
    {
        PrintHeader("2. RefundPayment — обробка Result<bool> через IsSuccess / Error");

        string[] refundRequests =
        {
            "TX-0001", 
            "TX-0001",  
            "TX-9999",
            "",        
            "TX-0003",  
        };

        foreach (string id in refundRequests)
        {
            Result<bool> result = service.RefundPayment(id);

            Console.Write($"\n  Повернення \"{id}\": ");
            if (result.IsSuccess)
                WriteColored("кошти повернено.", ConsoleColor.Green);
            else
                WriteColored($"відмова — {result.Error}", ConsoleColor.Yellow);

            Console.WriteLine($"    {result}");
        }

        // Демонстрація Result<T>.Failure(Exception)
        Console.WriteLine("\n  Result<T>.Failure(Exception):");
        Result<decimal> fromException = Result<decimal>.Failure(new TimeoutException("Шлюз не відповів за 30 с."));
        Console.WriteLine($"    IsSuccess = {fromException.IsSuccess}, Error = \"{fromException.Error}\"");
    }

    private static void PrintTransactions(PaymentService service)
    {
        PrintHeader("3. Підсумковий список транзакцій");

        IReadOnlyList<Transaction> transactions = service.GetTransactions();
        foreach (Transaction t in transactions)
            Console.WriteLine($"  {t}");

        decimal paid = transactions.Where(t => !t.IsRefunded).Sum(t => t.Amount);
        Console.WriteLine($"\n  Транзакцій: {transactions.Count}, сума без повернень: {paid:F2} грн");
    }

    private static void PrintHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 75));
        Console.WriteLine($" {title}");
        Console.WriteLine(new string('=', 75));
    }

    private static void WriteColored(string text, ConsoleColor color)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = previous;
    }
}
