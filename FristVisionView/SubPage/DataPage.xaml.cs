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
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
namespace VisionView
{
    /// <summary>
    /// _.xaml 的交互逻辑
    /// </summary>
    public partial class DataPage : UserControl
    {
        public DataPage()
        {
            InitializeComponent();
            //绘制图标
            Histogram.Series = new ISeries[]
            {
                //创建一个柱状图
                new ColumnSeries<double>
                {
                    Values = new double[]{100,200,100,2,199,88}
                },//注意这里是逗号
                //创建一个折线图
                new LineSeries<double>()
                {
                    Values = new double[]{100,200,300,100,200 }
                }
            };
            Pie1.Series = new ISeries[]
            {
                
                new PieSeries<double>
                {
                    InnerRadius = 140,
                    Name = "OK",
                    DataLabelsSize = 14,
                    Values = new double[]{100},
                    
                    CornerRadius = 40,
                    DataLabelsPosition = PolarLabelsPosition.Outer,
                    Fill = new SolidColorPaint(SKColor.Parse("#26fcff"))
                },
                new PieSeries<double>
                {
                    InnerRadius = 140,
                    Name = "NG",
                    CornerRadius = 20,
                    DataLabelsSize = 14,
                    Values = new double[]{50},
                    HoverPushout = 10,
                    Fill = new SolidColorPaint(SKColors.SteelBlue)
                },
                 new PieSeries<double>
                {
                    CornerRadius = 20,
                    InnerRadius = 140,
                    Name = "NG",
                    DataLabelsSize = 14,
                    Values = new double[]{150},
                    HoverPushout = 10,
                    Fill = new SolidColorPaint(SKColors.Green)
                }
            };
            Pie2.Series = new ISeries[]
            {

                new PieSeries<double>
                {
                    InnerRadius = 140,
                    Name = "OK",
                    DataLabelsSize = 14,
                    Values = new double[]{100},

                    CornerRadius = 40,
                    DataLabelsPosition = PolarLabelsPosition.Outer,
                    Fill = new SolidColorPaint(SKColor.Parse("#26fcff"))
                },
                new PieSeries<double>
                {
                    InnerRadius = 140,
                    Name = "NG",
                    CornerRadius = 20,
                    DataLabelsSize = 14,
                    Values = new double[]{50},
                    HoverPushout = 10,
                    Fill = new SolidColorPaint(SKColors.Brown)
                },
                 new PieSeries<double>
                {
                    CornerRadius = 20,
                    InnerRadius = 140,
                    Name = "NG",
                    DataLabelsSize = 14,
                    Values = new double[]{150},
                    HoverPushout = 10,
                    Fill = new SolidColorPaint(SKColors.Pink)
                }
            };

        
            
        }

        private void HistogramChart_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
