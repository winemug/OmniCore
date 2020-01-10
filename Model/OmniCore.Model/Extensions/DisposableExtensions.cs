using System;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Extensions
{
    public static class DisposableExtensions
    {
        public static void AutoDispose(this IDisposable disposable, IDisposableHandler disposableHandler)
        {
            if (!disposableHandler.Disposables.Contains(disposable))
                disposableHandler.Disposables.Add(disposable);
        }
    }
}