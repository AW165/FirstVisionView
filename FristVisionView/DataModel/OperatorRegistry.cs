using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls; // 必须引入反射库
using FirstVisionView.DataModel;
using FirstVisionView.ParameterUILibary.Core;//引入MenuTreeModel

namespace FirstVisionView.Core
{
    public static class OperatorRegistry
    {
        public static ObservableCollection<MenuCategory> GlobalMenuTree = new();
        public static void Initialize()
        {
            //如果菜单里有东西则直接返回，防止菜单重复
            if (GlobalMenuTree.Count != 0) return;
            //获取当前exe里面所有的类型
            Type[] allType = Assembly.GetExecutingAssembly().GetTypes();
            //筛选出贴了视觉标签的类，并分组
            var GroupedOpnrators = allType.Where(t => t.GetCustomAttribute<VisionOperatorAttribute>()! != null)
                .GroupBy(t => t.GetCustomAttribute<VisionOperatorAttribute>()!.Category);
            //循环这个组的成员，每个成员都有一个标志（DisPlayName,ParaType,Category）
            foreach (var group in GroupedOpnrators)
            {
                //创建一个新的菜单结构，当前的组名称就是GroupedOpnrators分好的key
                //内部结构相当于List(类别1(dictionary{Category,list(DisPlayName,ParaType)}),类别2...)
                var category = new MenuCategory { CategoryName = group.Key };
                //提取当前组的成员信息添加到菜单结构
                foreach (var type in group)
                {
                    //读取标签上的信息
                    var attr = type.GetCustomAttribute<VisionOperatorAttribute>();
                    //按照信息把这个类添加到上面的组里
                    category.SubOperator.Add(new MenuOperator { DispalyName = attr!.DispalyName, ParameterType = attr.ParameterType });
                    OperatorFactory.Register(attr.ParameterType, () =>
                    {
                        return (CommunityToolkit.Mvvm.ComponentModel.ObservableObject)Activator.CreateInstance(type)!;
                    });
                }
                GlobalMenuTree.Add(category);
            }
        }
    }
}