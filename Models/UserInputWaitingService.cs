namespace automation.mbtdistr.ru.Models
{
  public class UserInputWaitingService
  {
    private readonly Dictionary<long, (string Action, int EntityId)> _waiting = new();

    public void Register(long userId, string action, int entityId)
    {
      _waiting[userId] = (action, entityId);
    }

    public (string Action, int EntityId)? Get(long userId)
    {
      if (_waiting.TryGetValue(userId, out var data))
        return (data.Item1, data.Item2);
      else
        return null;
    }

    public void Remove(long userId)
    {
      _waiting.Remove(userId);
    }
  }
}
