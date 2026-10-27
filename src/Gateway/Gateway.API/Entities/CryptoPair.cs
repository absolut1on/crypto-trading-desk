namespace Gateway.API.Entities;

public class CryptoPair
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string BaseCurrency { get; set; } = string.Empty;
    public string QuoteCurrency { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
