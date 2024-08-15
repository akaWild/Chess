namespace MatchService.Exceptions
{
    public class MatchCancellationException(string message) : BaseClientException(message)
    {
    }
}
