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
using VisionView.SubPage.CameraParament;

namespace VisionView
{
    /// <summary>
    /// SettingPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingPage : UserControl
    {
        private CameraSetting _CameraNumPage1;
        private CameraSetting _CameraNumPage2;
        private CameraSetting _CameraNumPage3;
        private CameraSetting _CameraNumPage4;
        public SettingPage()
        {
            InitializeComponent();
            _CameraNumPage1 = new CameraSetting();
            CameraSetingPage.Content = _CameraNumPage1;
            CameraNumber.Items.Add(1);
            CameraNumber.Items.Add(2);
            CameraNumber.Items.Add(3);
            CameraNumber.Items.Add(4);
        }

        private void CameraSet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((int)CameraNumber.SelectedItem)
            {
                case 1:
                    {
                        if (_CameraNumPage1 != null)
                        {
                            _CameraNumPage1 = new CameraSetting();
                        }
                            CameraSetingPage.Content = _CameraNumPage1;
                        break; 
                    } 
                case 2:
                    {
                        if (_CameraNumPage2 != null)
                        {
                            _CameraNumPage2 = new CameraSetting();
                        }
                        CameraSetingPage.Content = _CameraNumPage2;
                        break;
                    }
            
                case 3:
                    {
                        if (_CameraNumPage3 != null)
                        {
                            _CameraNumPage3 = new CameraSetting();
                        }
                        CameraSetingPage.Content = _CameraNumPage3;
                        break;
                    }
                case 4:
                    {
                        if (_CameraNumPage4 != null)
                        {
                            _CameraNumPage4 = new CameraSetting();
                        }
                        CameraSetingPage.Content = _CameraNumPage4;
                        break;
                    }
            }

        }
    }
}
