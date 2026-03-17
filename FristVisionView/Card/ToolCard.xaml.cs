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
    }
}
