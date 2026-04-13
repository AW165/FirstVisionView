using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FirstVisionView.ParameterUILibary.Core;

namespace FirstVisionView.ParamenterUILibary.ParameterModel
{
    [ObservableObject]
    public partial class BaseParamenter
    {
        [ObservableProperty] private string _title = "";
        public ObservableCollection<BaseParameterItem> ParameterList { get; } = new();
    }
}

