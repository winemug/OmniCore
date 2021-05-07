using System;

namespace OmniCore.Model.Utilities.Extensions
{
    public static class DisposableExtensions
    {
        public static T DisposeWith<T>(this T disposable, ICompositeDisposableProvider provider)
            where T : IDisposable
        {
            provider.CompositeDisposable.Add(disposable);
            return disposable;
        }

    }
}