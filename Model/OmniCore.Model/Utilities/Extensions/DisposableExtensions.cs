using System;

namespace OmniCore.Model.Utilities.Extensions
{
    public static class DisposableExtensions
    {
        public static void AutoDispose(this IDisposable disposable, ICompositeDisposableProvider provider)
        {
            provider.CompositeDisposable.Add(disposable);
        }
    }
}