namespace MatchService.Exceptions
{
    public class MatchNotFoundException(string message) : BaseClientException(message)
    {
    }
}
