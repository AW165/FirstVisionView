using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VisionView.ParameterUILibary.Core;

namespace VisionView.ParamenterUILibary.ParameterModel
{
    //卡片参数
    /// <summary>
    /// BaseParameter类是所有算子类型的基类，定义了算子的输入参数和输出参数。
    /// </summary>
    public partial class BaseParameter : ObservableObject
    {
        public string Title { get; protected set; } = "";
        //定义一个输出池
        public Dictionary<string, ParameterDataType> OutParameter { get; } = new();
        public ObservableCollection<BaseParameterItem> ParameterList { get; } = new();
    }
}

