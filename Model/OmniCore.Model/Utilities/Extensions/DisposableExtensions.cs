using System;
using OmniCore.Model.Interfaces.Common;

namespace OmniCore.Model.Utilities.Extensions
{
    public static class DisposableExtensions
    {
        public static void AutoDispose(this IDisposable disposable, IDisposablesContainer disposablesContainer)
        {
            if (!disposablesContainer.Disposables.Contains(disposable))
                disposablesContainer.Disposables.Add(disposable);
        }
    }
}