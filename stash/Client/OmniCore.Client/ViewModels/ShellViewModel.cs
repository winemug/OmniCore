using Nito.AsyncEx.Synchronous;
using OmniCore.Client.Views;
using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Test;
using OmniCore.Model.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class ShellViewModel : BaseViewModel
    {
        public DataTemplate EmptyViewTemplate { get; }
        public DataTemplate TestControlViewTemplate { get; }
        public DataTemplate TestDetailViewTemplate { get; }
        public DataTemplate TestLogViewTemplate { get; }
        public DataTemplate SplashViewTemplate { get;  }

        public ShellViewModel(IClient client) : base(client)
        {
            EmptyViewTemplate = new DataTemplate(() => client.GetView<EmptyView>(true)
                .WaitAndUnwrapException());
            TestControlViewTemplate = new DataTemplate(() => client.GetView<TestControlView>(true)
                .WaitAndUnwrapException());
            TestDetailViewTemplate = new DataTemplate(() => client.GetView<TestDetailView>(true)
                .WaitAndUnwrapException());
            TestLogViewTemplate = new DataTemplate(() => client.GetView<TestLogView>(true)
                .WaitAndUnwrapException());
            SplashViewTemplate = new DataTemplate(() => client.GetView<SplashView>(true)
                .WaitAndUnwrapException());
        }
    }
}