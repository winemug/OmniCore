using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCore.Maui.Models;
using OmniCore.Maui.PageModels;
using PermissionsPageModel = OmniCore.Maui.PageModels.PermissionsPageModel;

namespace OmniCore.Maui.Pages;

public partial class PermissionsPage : ContentPage
{
    public PermissionsPage(PermissionsPageModel model)
    {
        InitializeComponent();
        this.WithModel(model);
    }
}