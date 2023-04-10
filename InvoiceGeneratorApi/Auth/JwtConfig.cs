namespace InvoiceGeneratorApi.Auth;

public class JwtConfig
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpireInMinutes { get; set;}
}