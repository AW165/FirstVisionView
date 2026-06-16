using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VisionView.DataModel;

namespace VisionView.Card
{
    /// <summary>
    /// ToolCard.xaml 的交互逻辑
    /// </summary>
    public partial class ToolCard : UserControl
    {
        public ToolCard()
        {
            InitializeComponent();
        }


        private void EditText_LostFocus(object sender, RoutedEventArgs e)
        {
            LockCard();
        }

        private void EditText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LockCard();
            }
        }

        private void EditText_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (EditText.Visibility == Visibility.Visible)
            {
                EditText.Focus();
                EditText.SelectAll();
            }
        }
        private void LockCard()
        {
            var card = this.DataContext as CardDataModel;
            if (card != null)
            {
                card.IsRenaming = false;
            }
        }
        private void PinLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//打断冒泡，防止触发卡片拖动与选择
            var clickedPin = sender as Ellipse;//尝试转化为椭圆
            if (clickedPin == null) return;//判断是否转换成功，失败返回
            AdjustPage BossPage = FindParent<AdjustPage>(clickedPin);//以此控件往上找到adjustPage类型的控件并返回
            if (BossPage == null) return;//检擦有无找到，没有则返回
            Point pinCenter = new Point(clickedPin.Width / 2, clickedPin.Height / 2);//获得椭圆的中心点，生成线段用到
            //前半段是生成了一个解释器，当前椭圆在adjustPage下的Paramentcanvas的绝对坐标（包含了阴影等），后半段是计算中心点位于paramentCanvas的绝对坐标
            Point absolutePoint = clickedPin.TransformToAncestor(BossPage.ParamentCanvas).Transform(pinCenter);
            BossPage.StartDrawingLine(absolutePoint, clickedPin.Name, this);//调用adjustPage下的StartDrawingLine方法，开始绘线
        }
        private void PinLeftButtonUp(object sender, MouseButtonEventArgs e)
        {



        }
        // 向上遍历 UI 视觉树，寻找指定类型的父类
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // 获取当前控件的上一层物理父类
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // 如果到顶了还没找到，就返回空
            if (parentObject == null) return null;

            // 检查这个父类是不是要找的类型（比如 AdjustPage）
            T parent = parentObject as T;
            if (parent != null)
                return parent; // 找到了！立刻返回！
            else
                return FindParent<T>(parentObject); // 没找到就把这个父类当成子类，继续往上找父类！(递归)
        }
    }
}
