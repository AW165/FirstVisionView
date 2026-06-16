using System.Net;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionView.DataModel
{
    public partial class WireDataModel : ObservableObject
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

            if (SourcePin == "RightPin") cp1.X += tension;
            else if (SourcePin == "LeftPin") cp1.X -= tension;
            else if (SourcePin == "TopPin") cp1.Y -= tension;
            else if (SourcePin == "BottomPin") cp1.Y += tension;

            if (EndPin == "RightPin") cp2.X += tension;
            else if (EndPin == "LeftPin") cp2.X -= tension;
            else if (EndPin == "TopPin") cp2.Y -= tension;
            else if (EndPin == "BottomPin") cp2.Y += tension;
            // 拼装 SVG 
            var curve = $"M {StartPoint.X},{StartPoint.Y} C {cp1.X},{cp1.Y} {cp2.X},{cp2.Y} {EndPoint.X},{EndPoint.Y}";
            // ==========================================
            double arrowLen = 12; // 箭头的长度
            double arrowWid = 6;  // 箭头的半宽（张开的角度）
            string arrow = "";

            // 终点是 LeftPin -> 线从左往右扎入 -> 箭头朝右 (>)
            if (EndPin == "LeftPin")
            {
                arrow = $" M {EndPoint.X - arrowLen},{EndPoint.Y - arrowWid} L {EndPoint.X},{EndPoint.Y} L {EndPoint.X - arrowLen},{EndPoint.Y + arrowWid}";
            }
            // 终点是 RightPin -> 线从右往左扎入 -> 箭头朝左 (<)
            else if (EndPin == "RightPin")
            {
                arrow = $" M {EndPoint.X + arrowLen},{EndPoint.Y - arrowWid} L {EndPoint.X},{EndPoint.Y} L {EndPoint.X + arrowLen},{EndPoint.Y + arrowWid}";
            }
            // 终点是 TopPin -> 线从上往下扎入 -> 箭头朝下 (V)
            else if (EndPin == "TopPin")
            {
                arrow = $" M {EndPoint.X - arrowWid},{EndPoint.Y - arrowLen} L {EndPoint.X},{EndPoint.Y} L {EndPoint.X + arrowWid},{EndPoint.Y - arrowLen}";
            }
            // 终点是 BottomPin -> 线从下往上扎入 -> 箭头朝上 (Λ)
            else if (EndPin == "BottomPin")
            {
                arrow = $" M {EndPoint.X - arrowWid},{EndPoint.Y + arrowLen} L {EndPoint.X},{EndPoint.Y} L {EndPoint.X + arrowWid},{EndPoint.Y + arrowLen}";
            }

            // 4. 将曲线和箭头无缝拼接到一起，发给显卡渲染！
            PathData = curve + arrow;
        }
    }
}
