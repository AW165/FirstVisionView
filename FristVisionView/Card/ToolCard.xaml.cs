using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstVisionView.DataModel;

namespace FirstVisionView.Card
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
            if ( card != null)
            {
                card.IsRenaming = false;
            }
        }

        private void PinLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//打断冒泡，防止触发卡片拖动与选择
            var clickedPin = sender as Ellipse;
            if (clickedPin == null) return;//判断
            AdjustPage BossPage =  FindParent<AdjustPage>(clickedPin);
            if (BossPage == null) return;
            Point pinCenter = new Point(clickedPin.Width / 2, clickedPin.Height / 2);
            Point absolutePoint = clickedPin.TransformToAncestor(BossPage.ParamentCanvas).Transform(pinCenter);
            BossPage.StartDrawingLine(absolutePoint,clickedPin.Name);
        }
        private void PinLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//打断冒泡，防止触发卡片拖动与选择


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
