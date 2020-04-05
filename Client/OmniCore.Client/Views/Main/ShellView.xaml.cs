using OmniCore.Client.Views.Base;
using OmniCore.Client.Views.Home;
using OmniCore.Client.Views.Test;
using OmniCore.Model.Interfaces.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OmniCore.Client.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ShellView : Shell
    {
        public ShellView(IViewPresenter viewPresenter)
        {
            InitializeComponent();

            EmptyView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            RadiosView = new DataTemplate(() => viewPresenter.GetView<RadiosView>(true));
            ActivePodsView = new DataTemplate(() => viewPresenter.GetView<ActivePodsView>(true));
            WaitingPodsView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            ArchivedPodsView = new DataTemplate(() => viewPresenter.GetView<EmptyView>(true));
            Test1View = new DataTemplate(() => viewPresenter.GetView<Test1View>(true));
            BindingContext = this;
        }

        public DataTemplate EmptyView { get; }
        public DataTemplate RadiosView { get; }
        public DataTemplate ActivePodsView { get; }
        public DataTemplate WaitingPodsView { get; }
        public DataTemplate ArchivedPodsView { get; }

        public DataTemplate Test1View { get; }
    }
}