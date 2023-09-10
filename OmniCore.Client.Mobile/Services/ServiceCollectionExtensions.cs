using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.Mobile.Services
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection
            AddViewViewModel<TPage, TViewModel>(this IServiceCollection serviceCollection)
        where TPage : Page
        where TViewModel : class, IViewModel
        {
            serviceCollection.AddTransient<TPage>();
            serviceCollection.AddTransient<TViewModel>();
            return serviceCollection;
        }

        //public static IServiceCollection
        //    AddTransient<TPage, TViewModel, TViewModelData>(this IServiceCollection serviceCollection)
        //    where TPage : Page
        //    where TViewModel : class, IViewModel<TViewModelData>
        //{
        //    serviceCollection.AddTransient<TPage>();
        //    serviceCollection.AddTransient<TViewModel>();
        //    return serviceCollection;
        //}
    }
}
