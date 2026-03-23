using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Kernel;

namespace FirstVisionView.DataModel
{
    public  partial class WireDataModel:ObservableObject
    {

        [ObservableProperty] private Point _startPoint;
        [ObservableProperty] private Point _endPoint;
        [ObservableProperty] private string _pathData = "";

        public void UpdatePath()
        {
            double tension = 50;
            Point cp1 = StartPoint;
            Point cp2 = EndPoint;

            // 简单假定：线都是从左往右流动的 (以后可以根据真实孔的方向优化)
            cp1.X += tension;
            cp2.X -= tension;

            // 拼装 SVG 
            PathData = $"M {StartPoint.X},{StartPoint.Y} C {cp1.X},{cp1.Y} {cp2.X},{cp2.Y} {EndPoint.X},{EndPoint.Y}";
        }
    }
}
