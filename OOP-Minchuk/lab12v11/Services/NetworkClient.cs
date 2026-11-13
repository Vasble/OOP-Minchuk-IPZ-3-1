using System.Net;

namespace lab18v11.Services;

public class NetworkClient
{
    private readonly int _transientFailures;
    private int _callCount;

    public NetworkClient(int transientFailures = 3)
    {
        _transientFailures = transientFailures;
    }

    public int CallCount => _callCount;

    public List<string> GetProductsFromApi(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException($"Некоректна адреса: {url}", nameof(url));

        _callCount++;

        Thread.Sleep(100);

        if (_callCount <= _transientFailures)
        {
            throw new HttpRequestException(
                $"Сервер {url} тимчасово недоступний (імітація, запит №{_callCount}).",
                inner: null,
                statusCode: HttpStatusCode.ServiceUnavailable);
        }

        return new List<string>
        {
            "Ноутбук Lenovo IdeaPad 5",
            "Смартфон Samsung Galaxy A55",
            "Навушники Sony WH-1000XM5",
            "Монітор Dell P2422H",
            "Клавіатура Logitech MX Keys",
        };
    }
}
