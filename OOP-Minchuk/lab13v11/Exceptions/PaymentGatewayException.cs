using System.Runtime.Serialization;

namespace lab19v11.Exceptions;

[Serializable]
public class PaymentGatewayException : Exception
{
    public string GatewayName { get; } = string.Empty;

    public string ErrorCode { get; } = string.Empty;

    public PaymentGatewayException()
        : base("Платіжний шлюз повернув помилку.")
    {
    }

    public PaymentGatewayException(string message)
        : base(message)
    {
    }

    public PaymentGatewayException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public PaymentGatewayException(string gatewayName, string errorCode, string message)
        : base(message)
    {
        GatewayName = gatewayName;
        ErrorCode = errorCode;
    }

    public PaymentGatewayException(string gatewayName, string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        GatewayName = gatewayName;
        ErrorCode = errorCode;
    }

    [Obsolete("Formatter-based serialization is obsolete.", DiagnosticId = "SYSLIB0051")]
    protected PaymentGatewayException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        GatewayName = info.GetString(nameof(GatewayName)) ?? string.Empty;
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? string.Empty;
    }

    [Obsolete("Formatter-based serialization is obsolete.", DiagnosticId = "SYSLIB0051")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(GatewayName), GatewayName);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }

    public override string ToString() =>
        $"{GetType().Name}: [{GatewayName}] {ErrorCode} — {Message}";
}
