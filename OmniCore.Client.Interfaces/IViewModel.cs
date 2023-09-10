using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IViewModel<in TData> : IViewModel
{
    ValueTask InitializeAsync(TData data);
}

public interface IViewModel : IAsyncDisposable
{
    ValueTask BindView(Page page);
}