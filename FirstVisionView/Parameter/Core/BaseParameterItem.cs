using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionView.ParameterUILibary.Core
{
    /// <summary>
    /// 这个类是所有控件类型的基类，定义了控件的基本属性和方法。
    /// 它继承自ObservableObject，使得属性的变化可以被观察和绑定。
    /// 该类包含控件的名称、可见性以及获取真实值的方法。
    /// 所有具体的控件类型如滑动条、开关、复选框、组合框和文本框都继承自这个基类，并实现了获取真实值的方法。
    /// 
    /// </summary>
    public abstract partial class BaseParameterItem : ObservableObject
    {
        [ObservableProperty] private string _name = "";
        [ObservableProperty] private bool _isVisible = true;
        //存储指定的类型
        [ObservableProperty] private ParameterDataType _acceptType;
        //存储继承的选项列表
        [ObservableProperty] private ObservableCollection<InputOptionModel> _options = new();
        [ObservableProperty] private bool _isReference = false;
        [ObservableProperty] private string referenceSourceId = "";
        //指定abstract修饰符，不能被实例化，只能被继承,设置获取真实值的方法，子类必须实现
        public abstract object GetRealValue();
    }
    /// <summary>
    /// SliderParameterItem类是滑动条控件的具体实现，继承自BaseParameterItem。
    /// </summary>
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
    /// <summary>
    /// SwitchParameterItem类是开关控件的具体实现，继承自BaseParameterItem。
    /// </summary>
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
    /// <summary>
    /// CheckBoxParameterItem类是复选框控件的具体实现，继承自BaseParameterItem。
    /// </summary>
    public partial class CheckBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private bool _isOpen;
        public override object GetRealValue()
        {
            return IsOpen;
        }
    }
    /// <summary>
    /// ComboboxParameterItem类是组合框控件的具体实现，继承自BaseParameterItem。
    /// </summary>
    public partial class ComboboxParameterItem : BaseParameterItem
    {

        [ObservableProperty] private string _selectedValue = "";
        public override object GetRealValue()
        {
            return SelectedValue;
        }
    }
    /// <summary>
    /// TextBoxParameterItem类是文本框控件的具体实现，继承自BaseParameterItem。
    /// </summary>
    public partial class TextBoxParameterItem : BaseParameterItem
    {
        [ObservableProperty] private int _presentValue;
        public override object GetRealValue()
        {
            return PresentValue;
        }
    }
}
