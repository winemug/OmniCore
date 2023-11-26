using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCore.Client.ViewModels;

public abstract class DataViewModel<T> : ViewModel
{
    public abstract Task LoadDataAsync(T data);
}