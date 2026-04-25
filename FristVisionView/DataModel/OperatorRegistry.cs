using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection; // 必须引入反射库
using FirstVisionView.DataModel; // 引入你建好的 MenuCategory 和 MenuOperator

namespace FirstVisionView.Core
{
    public static class OperatorRegistry
    {
        // 全局唯一的菜单树，供 XAML 绑定
        public static ObservableCollection<MenuCategory> GlobalMenuTree { get; } = new();

        // 软件启动时只需调用一次此方法
        public static void Initialize()
        {
            // 防呆设计：如果已经有数据了，绝不重复扫描
            if (GlobalMenuTree.Count > 0) return;

            // ==========================================
            // 🌟 核心语法 1：全仓盘点
            // Assembly.GetExecutingAssembly()：获取当前正在运行的程序集(即你的 exe)
            // GetTypes()：把程序集里所有的 Class、Enum、Interface 全捞出来
            // ==========================================
            Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            // ==========================================
            // 🌟 核心语法 2：激光筛选与打包分拣 (LINQ)
            // Where: 只保留能成功拿到 VisionOperatorAttribute 标签的类
            // GroupBy: 按照标签里的 Category (如"图像处理") 进行分组装箱
            // ==========================================
            var groupedOperators = allTypes
                .Where(t => t.GetCustomAttribute<VisionOperatorAttribute>() != null)
                .GroupBy(t => t.GetCustomAttribute<VisionOperatorAttribute>()!.Category);

            // ==========================================
            // 🌟 核心语法 3：双层 foreach 组装 UI 树
            // ==========================================
            foreach (var group in groupedOperators)
            {
                // group.Key 就是分类名 (例如 "图像处理")
                var category = new MenuCategory { CategoryName = group.Key };

                foreach (Type type in group) // 遍历这个分类下的每一个具体的算子类
                {
                    // 撕下这块代码头上的标签，看看上面写了什么
                    var attr = type.GetCustomAttribute<VisionOperatorAttribute>()!;

                    // 动作 A：把名字和 ID 抄到宣传册(MenuOperator)上，塞进大类的肚子里
                    category.SubOperators.Add(new MenuOperator
                    {
                        DisplayName = attr.DisplayName,
                        CardType = attr.CardType
                    });

                    // 动作 B：教工厂怎么制造这个对象
                    // Activator.CreateInstance(type) 是反射引擎的大招！
                    // 它能在不知道类名的情况下，拿着 Type 图纸强行 new 出一个实例。
                    OperatorFactory.Register(attr.CardType, () =>
                    {
                        return (ObservableObject)Activator.CreateInstance(type)!;
                    });
                }

                // 把装好所有小算子的大类，推入全局展示树
                GlobalMenuTree.Add(category);
            }
        }
    }
}