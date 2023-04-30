namespace InvoiceGeneratorApi.Providers;

public class UserInfo
{
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public string UserName { get; set; }

    public UserInfo(int userId, string userEmail, string userName)
    {
        UserId = userId;
        UserEmail = userEmail;
        UserName = userName;
    }
}