using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Test;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;

namespace OmniCore.Client.ViewModels.Base
{
    public class ShellViewModel : BaseViewModel
    {
        public DataTemplate EmptyView { get; }
        public DataTemplate RadiosView { get; }
        public DataTemplate ActivePodsView { get; }
        public DataTemplate WaitingPodsView { get; }
        public DataTemplate ArchivedPodsView { get; }
        public DataTemplate Test1View { get; }
        
        public ShellViewModel(IClient client) : base(client)
        {
            EmptyView = new DataTemplate(() => client.GetView<EmptyView>(true));
            RadiosView = new DataTemplate(() => client.GetView<RadiosView>(true));
            ActivePodsView = new DataTemplate(() => client.GetView<ActivePodsView>(true));
            WaitingPodsView = new DataTemplate(() => client.GetView<EmptyView>(true));
            ArchivedPodsView = new DataTemplate(() => client.GetView<EmptyView>(true));
            Test1View = new DataTemplate(() => client.GetView<Test1View>(true));
        }
    }
}