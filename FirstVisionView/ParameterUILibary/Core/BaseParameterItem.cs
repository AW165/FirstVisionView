using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionView.ParameterUILibary.Core
{
    /// <summary>
    /// 这个类是所有参数类型的基类，定义了参数的基本属性和方法。
    /// 它继承自ObservableObject，使得属性的变化可以被观察和绑定。
    /// 该类包含参数的名称、可见性以及获取真实值的方法。
    /// 所有具体的参数类型（如滑动条、开关、复选框、组合框和文本框）都继承自这个基类，并实现了获取真实值的方法。
    /// </summary>
    public abstract partial class BaseParameterItem : ObservableObject//参数基类，所有参数类型都继承自该类，指定abstract修饰符，不能被实例化，只能被继承
    {
        [ObservableProperty] private string _name = "";
        [ObservableProperty] private bool _isVisible = true;
        public abstract object GetRealValue();//设置获取真实值的方法，子类必须实现
    }
    public partial class SliderParameterItem : BaseParameterItem
    {
        [ObservableProperty] private double _value;
        [ObservableProperty] private double _max;
        [ObservableProperty] private double _min;
        [ObservableProperty] private double _step = 1;
        public override object GetRealValue()
        {
            return Value;
        }
    }
    public partial class SwitchParameterItem : BaseParameterItem
    {
        [ObservableProperty] private bool _isOn;
        [ObservableProperty] private string _onText = "已启用";
        [ObservableProperty] private string _offText = "已关闭";
        public override object GetRealValue()
        {
            return IsOn;
        }
    }
    public partial class CheckBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private ParameterDataType _acceptType;
        [ObservableProperty] private bool _isOpen;
        public override object GetRealValue()
        {
            return IsOpen;
        }
    }
    public partial class ComboboxParameterItem : BaseParameterItem
    {
        //存储指定的类型
        [ObservableProperty] private ParameterDataType _acceptType;
        //存储Combobox的选项列表
        [ObservableProperty] private ObservableCollection<InputOptionModel> _options = new();
        [ObservableProperty] private string _selectedValue = "";
        public override object GetRealValue()
        {
            return SelectedValue;
        }
    }
    public partial class TextBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private ParameterDataType _accptType;
        [ObservableProperty] private int _presentValue;
        public override object GetRealValue()
        {
            return PresentValue;
        }
    }
}
