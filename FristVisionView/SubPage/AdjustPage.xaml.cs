// 引入系统基础类库，提供基础数据类型和底层运算支持
using System;
// 引入泛型集合库，为我们提供 Dictionary（字典）等高级数据结构
using System.Collections.Generic;
// 引入 LINQ 查询语言，让我们能像查数据库一样查内存里的集合（比如用 Where, Any）
using System.Linq;
// 引入 WPF 核心基础库，提供 Point（坐标点）、Rect（矩形）、依赖属性等基础对象
using System.Windows;
// 引入 WPF 控件库，提供 UserControl（用户控件）、Canvas（画布）等 UI 元素
using System.Windows.Controls;
// 引入 WPF 输入控制库，提供鼠标事件（MouseEventArgs）、键盘按键（Key）的监听能力
using System.Windows.Input;
using FirstVisionView.Card;

// 引入自己的数据模型库，拿到 CardDataModel
using FirstVisionView.DataModel;
// 引入自己的视图模型库，拿到AdjustViewModel
using FirstVisionView.ViewModels;
namespace FirstVisionView
{
    public partial class AdjustPage : UserControl
    {
        public AdjustPage()
        {
            // 这是 WPF 必须调用的方法，负责把 XAML 里的 UI 元素解析并绘制到屏幕上
            InitializeComponent();
        }
        //引用ViewModel，拿到vm的数据

        private AdjustViewModel? vm => this.DataContext as AdjustViewModel;
        //拖拽状态
        private bool _IsDragging= false;
        //鼠标移动状态
        private bool _IsMoving = false;
        //鼠标左键在Canvas按下状态
        private bool _IsCanvasLeftDown = false;
        //鼠标左键在Card按下状态
        private bool _IsCardLeftDown = false;
        //鼠标右键按下状态
        private bool _IsRightDown = false;
        //鼠标滚轮状态
        private bool _IsWheel = false;
        //记录鼠标点击在Canvas且拖拽的状态
        private bool _IsCanvasSeleted= false;
        //记录鼠标点击Canvas时的当前坐标
        private Point _CanvasStartPoint;
        //记录鼠标点击Card时的当前坐标
        private Point _CardStartPoint;
        //记录鼠标点击Card时的当前坐标
        private Point _CardMousePoint;
        //记录鼠标点击Card
        private ToolCard _CurrentCard;
        //记录卡片与坐标点
        private Dictionary<CardDataModel,Point> _DragStartPoint;
        // 标记在右键按下期间，是否真正发生了拖拽动作（用于区分“右键呼出菜单”和“右键拖动画布”）。
        private bool _hasPanned;
        // 记录开始平移画布时，鼠标在屏幕上的物理初始坐标点。
        private Point _PanStartPoint;
        // 记录开始平移画布时，底部水平滚动条的初始偏移量数值。
        private double _PanStartOffsetX;
        // 记录开始平移画布时，右侧垂直滚动条的初始偏移量数值。
        private double _PanStartOffsetY;
        // 记录当前整个大画布的缩放倍率，默认 1.0 代表 100% 原始大小。
        private double _currentZoom = 1.0;
        // 防手抖的像素阈值：鼠标按下后移动超过 5 个像素，才被正式认定为“拖拽行为”，否则视为原地点击。
        private const int DistanceThreshold = 5;


        //============================Canvas事件区域======================
        //Canvas上左键点击
        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            _IsCanvasLeftDown = true;//设置左键点击状态
            if (vm == null) return;//如果没持有vm则直接退出
            vm.ClearSeletionStatusCommand.Execute(null);//清除卡片高亮状态
            _CanvasStartPoint = e.GetPosition(ParamentCanvas);
            ParamentCanvas.CaptureMouse();
        }
        //Canvas上左键松开
        private void CanvasLeftUp(object sender, MouseButtonEventArgs e)
        {
            _IsCanvasLeftDown = false;//设置左键松开
            _IsDragging = false;//设置拖拽状态结束
            SelectionBox.Visibility = Visibility.Collapsed;//矩形框关闭
            ParamentCanvas.ReleaseMouseCapture();//释放鼠标

        }
        //Canvas上右键点击
        private void CanvasRightDown(object sender, MouseButtonEventArgs e)
        {
            _IsRightDown = true;//设置右键按下状态
        }
        //Canvas上右键松开
        private void RightUp(object sender, MouseButtonEventArgs e)
        {
            _IsRightDown=false;//取消右键按下状态
            _IsDragging = false;//取消拖拽状态
            if (_hasPanned == true)
            {
                _hasPanned = false;
                return;//刚才在拖拽,不弹出菜单
            }
            if (vm == null) return;
            var cardList = vm.AllCards.Where(c => c.IsSelected).ToList();//判断有无卡片被选中，有则弹出删除菜单，否则弹出添加菜单
            if (cardList.Count == 0)
            {
                AddCardPopup();

                return;

            }
            else DeletePopup();



        }
        //Canvas上鼠标移动
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_IsCanvasLeftDown != true) return;
            

            var currentPoint = e.GetPosition(ParamentCanvas);
            var  disX = Math.Abs(currentPoint.X - _CanvasStartPoint.X);
            var  disY = Math.Abs(currentPoint.Y - _CanvasStartPoint.Y);
            if (disX >= DistanceThreshold || disY >= DistanceThreshold)
            {
                _hasPanned = true; //超过阈值就设置移动状态
            }
            if (_hasPanned == false) return;
            else
            {
                //此处为画框逻辑
                var rectangleX=Math.Min(_CanvasStartPoint.X,currentPoint.X);
                var rectangleY=Math.Min(_CanvasStartPoint.Y,currentPoint.Y);
                var rectangleWidth = Math.Abs(currentPoint.X - _CanvasStartPoint.X);
                var rectangleHeight = Math.Abs(currentPoint.Y - _CanvasStartPoint.Y);
                Rect rect1 = new Rect(rectangleX, rectangleY, rectangleWidth, rectangleHeight);
                SelectionBox.Width = rectangleWidth;
                SelectionBox.Height = rectangleHeight;
                Canvas.SetLeft(SelectionBox,rectangleX);
                Canvas.SetTop(SelectionBox,rectangleY);
                SelectionBox.Visibility=Visibility.Visible;
                foreach (var card in vm.AllCards)
                {
                    Rect rect2 = new Rect(card.X,card.Y,150,80);
                    if (rect1.IntersectsWith(rect2) == true)
                    {
                        vm.AddSeletionStatusCommand.Execute(card);
                    }
                    else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                    {
                       //按下control时旧的状态不清

                    }
                    else
                    {
                        vm.ClearSeletionStatusCommand.Execute(card);//没按下control时清除之前的卡片状态
                    }

                }
                
            }
        }

        //========================Card事件区域====================
        
        //Card上左键点击
        private void CardLeftDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//打断冒泡
            var currentCard = sender as ToolCard;
            if (currentCard == null) return;
            _CurrentCard = currentCard;
            _IsCardLeftDown = true;//设置在卡片按下状态
            _CardStartPoint = e.GetPosition(ParamentCanvas);//记录鼠标位于画布上的位置
            _CardMousePoint = e.GetPosition(ParamentCanvas);//记录鼠标位于卡片内部的位置
            var card = sender as CardDataModel;//转换类型
            if (card == null || vm == null) return;
            _DragStartPoint[card] = _CardMousePoint;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                vm.ClearSeletionStatusCommand.Execute(null);//没按下control时清除之前的卡片状态
                _DragStartPoint.Clear();//清除记录的卡片坐标
            }
            vm.AddSeletionStatusCommand.Execute(card);//设置状态
                //按下control时旧的状态不清且追加 
            _CurrentCard.CaptureMouse();//捕获鼠标

        }
        //Card上左键松开
        private void CardLeftUp(object sender, MouseButtonEventArgs e)
        {
            _IsCardLeftDown = false;//设置松开状态
            _IsDragging = false;//设置拖拽状态结束
            _CurrentCard.ReleaseMouseCapture();
        }
        //Card上鼠标移动
        private void CardMouseMove(object sender, MouseEventArgs e)
        {
            
            
            if (_IsCardLeftDown != true) return;
            var currentPoint = e.GetPosition(ParamentCanvas);
            var disX = Math.Abs(currentPoint.X - _CardStartPoint.X);
            var disY = Math.Abs(currentPoint.Y - _CardStartPoint.Y);
            if (disX >= DistanceThreshold || disY >= DistanceThreshold)
            {
                _hasPanned = true; //超过阈值就设置移动状态
            }
            if (_hasPanned == false) return;
            //此处添加移动卡片逻辑
            foreach (var card in _DragStartPoint)
            {
                var cardKey = card.Key;
                var cardPoint = card.Value;
                cardKey.X = currentPoint.X -  _CardMousePoint.X;
                cardKey.Y = currentPoint.Y -  _CardMousePoint.Y;
            }

        }

        private void LeftScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }

        private void LeftScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void LeftScrollViewer_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void LeftScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void DeleCard(object sender, RoutedEventArgs e)
        {

        }


        //==============================菜单/公共事件=======================

        private void DeletePopup()
        {
            DeleteCard.IsOpen = true;
        }
        private void AddCardPopup()
        {
            AddCard.IsOpen = true;
        }
    }

}