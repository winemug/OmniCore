using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OmniCore.Mobile.ViewModels;
using Xamarin.Forms;

namespace OmniCore.Mobile.Services
{
    public class ViewModelBindingRegistry
    {
        private Dictionary<Type, Type> _typeDict = new Dictionary<Type, Type>();
        private Dictionary<Page, BaseViewModel> _instanceDict = new Dictionary<Page, BaseViewModel>();

        public void RegisterModelBinding<TPage, TViewModel>()
        {
            _typeDict.Add(typeof(TPage), typeof(TViewModel));
        }
        
        public BaseViewModel GetViewModelForInstance(Page page)
        {
            _instanceDict.TryGetValue(page, out var vmInstance);
            if (vmInstance != null)
            {
                return vmInstance;
            }
            
            _typeDict.TryGetValue(page.GetType(), out var vmType);
            if (vmType != null)
            {
                var instance = Activator.CreateInstance(vmType) as BaseViewModel;
                _instanceDict.Add(page, instance);
                return instance;
            }

            return null;
        }
    }
}