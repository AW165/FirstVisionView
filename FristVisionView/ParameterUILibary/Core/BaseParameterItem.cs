using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FirstVisionView.ParameterUILibary.Core
{
    public partial class BaseParameterItem : ObservableObject
    {
        [ObservableProperty] private string _name = "";
    }
    public partial class SliderParameterItem : BaseParameterItem
    {
        [ObservableProperty] private double _value;
        [ObservableProperty] private double _max;
        [ObservableProperty] private double _min;
        [ObservableProperty] private double _step = 1;
    }
    public partial class SwitchParameterItem : BaseParameterItem
    {
        [ObservableProperty] private bool _isOn;
        [ObservableProperty] private string _onText = "已启用";
        [ObservableProperty] private string _offText = "已关闭";
    }
    public partial class CheckBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private ParameterDataType _acceptType;
        [ObservableProperty] private bool _isOpen;
    }
    public partial class ComboboxParameterItem : BaseParameterItem
    {
        //存储指定的类型
        [ObservableProperty] private ParameterDataType _acceptType;
        //存储Combobox的选项列表
        [ObservableProperty] private ObservableCollection<string> _options = new();
        [ObservableProperty] private string _selectedValue = "";
    }
    public partial class TextBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private ParameterDataType _accptType;
        [ObservableProperty] private int _presentValue;
    }
}
