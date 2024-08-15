namespace MatchService.Exceptions
{
    public class DuplicateMatchException(string message) : BaseClientException(message)
    {
    }
}
