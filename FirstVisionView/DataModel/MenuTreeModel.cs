using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionView.DataModel
{
    /// <summary>
    /// 中转类，用来转存反射的内容，提供给xaml来绑定，下面存储为大类类名，且Xaml的HierarchicalDataTemplate要求必须提供层级结构
    /// </summary>
    public class MenuCategory
    {
        public string CategoryName{ get; set; }//存储大类类名
        public ObservableCollection<MenuOperator> SubOperator { get; set; } = new();//把MenuOperator作为子集暴露给Xaml来绑定
    }
    /// <summary>
    /// 中转类，用来转存反射的内容，提供给xaml来绑定，下面存储为按钮名称和实例化算子类型
    /// 在这里可以扩展更多的需求，比如加icon，这个类把他们组装在一起，填充ObservableCollection<T>，T必须是一个类型
    /// </summary>
    public class MenuOperator
    {
        public string DispalyName { get; set; }//存储按钮显示的名称
        public string ParameterType { get; set; }//存储算子类型
    }
    
}
