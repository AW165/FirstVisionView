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
    public partial class BaseParamenter : ObservableObject
    {
        public string Title { get; protected set; } = "";
        public ObservableCollection<BaseParameterItem> ParameterList { get; } = new();
    }
}

