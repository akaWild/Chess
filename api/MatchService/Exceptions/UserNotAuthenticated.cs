namespace MatchService.Exceptions
{
    public class UserNotAuthenticated(string message) : BaseClientException(message)
    {
    }
}
