namespace InvoiceGeneratorApi.Providers;

public class UserRequestProvider : IUserRequestProvider
{
    private readonly HttpContext _httpContext;

    public UserRequestProvider(IHttpContextAccessor contextAccessor)
    {
        _httpContext = contextAccessor.HttpContext;
    }
    public UserInfo GetUserInfo()
    {
        if (_httpContext.User.Claims.Any())
        {
            var userId = int.Parse(_httpContext.User.Claims.First(c => c.Type == "userId").Value);
            var userEmail = _httpContext.User.Claims.First(c => c.Type == "userEmail").Value;
            var userName = _httpContext.User.Identity.Name;
            return new UserInfo(userId, userEmail, userName);
        }

        return new UserInfo(0, "", "");
    }
}