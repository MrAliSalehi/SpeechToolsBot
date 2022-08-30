namespace SpeechToolsBot.Common.Extensions;

internal sealed class MethodResponseCacher<TIn, TOut> where TIn : notnull where TOut : notnull
{
    private readonly Func<TIn, TOut> _func;
    private readonly Dictionary<TIn, TOut> _tDictionary = new();

    public MethodResponseCacher(Func<TIn, TOut> func)
    {
        _func = func;
    }

    public TOut Invoke(TIn tin) => _tDictionary.TryGetValue(tin, out var value) ? value : _tDictionary[tin] = _func(tin);
}