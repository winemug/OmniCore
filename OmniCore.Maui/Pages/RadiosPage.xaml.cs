using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Maui.PageModels;

namespace OmniCore.Maui.Pages;

public partial class RadiosPage : ContentPage
{
    public RadiosPage(RadiosPageModel model)
    {
        InitializeComponent();
        this.WithModel(model);
    }
}