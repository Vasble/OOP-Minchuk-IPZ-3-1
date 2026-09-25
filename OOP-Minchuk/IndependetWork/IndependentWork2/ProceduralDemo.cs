
// ПРОБЛЕМИ ПРОЦЕДУРНОЇ ВЕРСІЇ (Завдання 3)

// 1. Дані про ОДИН товар розкидані по трьох окремих (паралельних) масивах —
//    names, prices, quantities. Немає єдиної сутності "товар": щоб дізнатись
//    все про товар №2, треба вручну звертатись до трьох різних масивів
//    за одним і тим самим індексом.
//
// 2. Немає жодної гарантії, що масиви мають однакову довжину. Якщо хтось
//    додасть елемент лише в один з масивів (наприклад, забуде додати
//    кількість для нового товару), компілятор цього не помітить —
//    помилка (IndexOutOfRangeException або зсув даних) з'явиться лише
//    під час виконання.
//
// 3. Логіку знижки (ApplyDiscount) неможливо перевикористати в іншому
//    сценарії
//
// 4. Додавання нового товару вимагає СИНХРОННОЇ зміни одразу трьох
//    масивів у правильному порядку — легко помилитись і "зсунути" дані.
//
// 5. Немає інкапсуляції: будь-яка частина програми може змінити масив
//    цін окремо від масиву назв, і ніщо не захищає узгодженість даних.


public static class ProceduralDemo
{
    public static void Run()
    {
        Console.WriteLine("=== Процедурна версія ===");

        string[] names = { "Ноутбук", "Мишка", "Клавіатура", "Навушники", "Флешка" };
        double[] prices = { 25000, 350, 800, 600, 250 };
        int[] quantities = { 1, 2, 1, 1, 3 };

        double[] lineTotals = CalculateLineTotals(names, prices, quantities);
        double[] discountedTotals = ApplyDiscount(prices, lineTotals);

        for (int i = 0; i < names.Length; i++)
        {
            string discountMark = prices[i] > 500 ? " (знижка 10%)" : "";
            Console.WriteLine($"{names[i]}: {discountedTotals[i]:F2} грн{discountMark}");
        }

        double total = CalculateCartTotal(discountedTotals);
        Console.WriteLine($"Підсумок кошика (процедурна версія): {total:F2} грн");
        Console.WriteLine();
    }

    static double[] CalculateLineTotals(string[] names, double[] prices, int[] quantities)
    {
        double[] totals = new double[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            totals[i] = prices[i] * quantities[i];
        }
        return totals;
    }

    static double[] ApplyDiscount(double[] prices, double[] lineTotals)
    {
        double[] result = new double[lineTotals.Length];
        for (int i = 0; i < lineTotals.Length; i++)
        {
            result[i] = prices[i] > 500 ? lineTotals[i] * 0.9 : lineTotals[i];
        }
        return result;
    }

    static double CalculateCartTotal(double[] discountedTotals)
    {
        double total = 0;
        foreach (double t in discountedTotals)
        {
            total += t;
        }
        return total;
    }
}
