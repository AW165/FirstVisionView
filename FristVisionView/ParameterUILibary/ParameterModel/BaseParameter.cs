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
    //卡片参数
    public partial class BaseParameter : ObservableObject
    {
        public string Title { get; protected set; } = "";
        //定义一个输出池
        public Dictionary<string,ParameterDataType> OutParameter { get;} = new();
        public ObservableCollection<BaseParameterItem> ParameterList { get; } = new();
    }
}

