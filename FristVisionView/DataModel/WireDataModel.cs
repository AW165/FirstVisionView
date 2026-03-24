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

        [ObservableProperty] private Point _startPoint;//开始点
        [ObservableProperty] private Point _endPoint;//结束点
        [ObservableProperty] private string _pathData = "";
        [ObservableProperty] private CardDataModel _sourceCard;//CardDataModel类型可以直接拿到卡片的位置数据，这样线可以实时获取跟踪卡片来移动
        [ObservableProperty] private CardDataModel _targetCard;
        [ObservableProperty] private string _sourcePin;//起始连接的端口
        [ObservableProperty] private string _endPin;//结束连接的端口
        [ObservableProperty] private bool _isSelected = false;//选中状态  


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
