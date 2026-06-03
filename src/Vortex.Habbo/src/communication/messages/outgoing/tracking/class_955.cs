using Vortex.Core.Communication.Messages;

namespace Vortex.Habbo.Communication.Messages.Outgoing.Tracking;

public class EventLogMessageComposer(string param1, string param2, string param3, string param4 = "", int param5 = 0)
    : IMessageComposer
{
    private readonly string _action = param3 ?? string.Empty;
    private readonly string _extraString = param4 ?? string.Empty;
    private readonly string var_329 = param2 ?? string.Empty;
    private readonly string var_638 = param1 ?? string.Empty;

    public void Dispose() { }

    public List<object> GetMessageArray()
    {
        return
        [
            var_638,
            var_329,
            _action,
            _extraString,
            param5,
        ];
    }
}
