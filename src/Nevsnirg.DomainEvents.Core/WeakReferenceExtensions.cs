namespace Nevsnirg.DomainEvents.Core;
internal static class WeakReferenceExtensions
{
    internal static void IfRefIsStrong<TIn>(this WeakReference<TIn> weakRef, Action<TIn> @do, Action @else)
        where TIn : class
    {
        if (weakRef.TryGetTarget(out var strongRef))
            @do(strongRef);
        else
            @else();
    }

    internal static TOut IfRefIsStrong<TIn, TOut>(this WeakReference<TIn> weakRef, Func<TIn, TOut> @do, Func<TOut> @else)
        where TIn : class
    {
        if (weakRef.TryGetTarget(out var strongRef))
            return @do(strongRef);
        else
            return @else();
    }
}