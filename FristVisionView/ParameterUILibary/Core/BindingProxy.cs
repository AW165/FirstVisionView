using System.Windows;

namespace VisionView.Core
{
    // 【设问】：为什么要继承？难道我们要冻结什么东西吗？
    // 【认领】：Freezable 是 WPF 框架留下的一个“合法后门”。它最初是设计给画刷（Brush）用的，为了让同一个红色画刷能同时被 100 个不同的按钮使用。
    //           正因如此，Freezable 天生具备“穿越任何视觉树边界而不受限制”的超能力！我们借用了它的壳。
    public class BindingProxy : Freezable
    {
        // 【设问】：为什么必须重写 CreateInstanceCore 方法？
        // 【认领】：这是继承 Freezable 的强制契约。当 WPF 在后台克隆或者传递这个基站时，它需要知道怎么 new 一个新的出来。
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        // 【设问】：这个 Data 属性是干什么用的？
        // 【认领】：这就是量子基站的“储物箱”。由于我们要装的是 ViewModel（具体的 AdjustViewModel），为了通用性，我们把它定义为 object 类型。
        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // 【设问】：为什么不写成普通的 { get; set; }，而非要搞个吓人的 DependencyProperty？
        // 【认领】：普通的属性无法参与 WPF 的 XAML 数据绑定（Binding）！只有被注册为“依赖属性（DependencyProperty）”，它才能在 XAML 里接收 `Data="{Binding}"` 传过来的 ViewModel。
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}