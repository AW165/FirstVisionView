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
using System.Windows.Media;
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
        private double gridSize = 10;
        private AdjustViewModel? vm => this.DataContext as AdjustViewModel;
        //画框状态
        private bool _IsDragging = false;
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
        private bool _IsCanvasSeleted = false;
        //记录鼠标点击Canvas时的当前坐标
        private Point _CanvasStartPoint;
        //记录鼠标点击Card时的当前坐标
        private Point _CardStartPoint;
        //记录鼠标点击Card时的当前坐标
        private Point _CardMousePoint;
        //记录鼠标点击Card
        private ToolCard _CurrentCard;
        //记录卡片与坐标点
        private Dictionary<CardDataModel, Point> _DragStartPoint = new();
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
        private Point _PanStartMousePos;
        private Point _PanStartTranslate; // 记录按下时，画布原本的偏移量
        //============================Canvas事件区域======================
        //Canvas上左键点击
        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            ParamentCanvas.Focus();
            _IsCanvasLeftDown = true;//设置左键点击状态
            _DragStartPoint.Clear();//清空所有的选择卡片
            if (vm == null) return;//如果没持有vm则直接退出
            bool isControl = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);//检查有无按下control键
            if (!isControl) vm.ClearSeletionStatusCommand.Execute(null);//清除卡片高亮状态
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

            _IsRightDown = false;//取消右键按下状态

            _IsDragging = false;//取消拖拽状态

            if (vm == null) return;
            var cardList = vm.AllCards.Where(c => c.IsSelected).ToList();//判断有无卡片被选中，有则弹出删除菜单，否则弹出添加菜单
            if (cardList.Count == 0)
            {
                Point logicalPos = e.GetPosition(ParamentCanvas);
                vm.CurrentMousePoint = new System.Drawing.PointF((float)logicalPos.X, (float)logicalPos.Y); // 假设你在 ViewModel 里用 PointF 或者 WPF的Point 存它
                vm.AddPopup = true;
                return;
            }
            else vm.DelePopup = true;
        }

        //Canvas上鼠标移动

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (_IsCanvasLeftDown != true) return;
            var currentPoint = e.GetPosition(ParamentCanvas);
            var disX = Math.Abs(currentPoint.X - _CanvasStartPoint.X);
            var disY = Math.Abs(currentPoint.Y - _CanvasStartPoint.Y);
            if (disX >= DistanceThreshold || disY >= DistanceThreshold)
            {
                _hasPanned = true; //超过阈值就设置移动状态
            }
            if (_hasPanned == false) return;
            else
            {
                //此处为画框逻辑
                var rectangleX = Math.Min(_CanvasStartPoint.X, currentPoint.X);
                var rectangleY = Math.Min(_CanvasStartPoint.Y, currentPoint.Y);
                var rectangleWidth = Math.Abs(currentPoint.X - _CanvasStartPoint.X);
                var rectangleHeight = Math.Abs(currentPoint.Y - _CanvasStartPoint.Y);
                Rect rect1 = new Rect(rectangleX, rectangleY, rectangleWidth, rectangleHeight);
                SelectionBox.Width = rectangleWidth;
                SelectionBox.Height = rectangleHeight;
                Canvas.SetLeft(SelectionBox, rectangleX);
                Canvas.SetTop(SelectionBox, rectangleY);
                SelectionBox.Visibility = Visibility.Visible;
                foreach (var card in vm.AllCards)
                {

                    Rect rect2 = new Rect(card.X, card.Y, 150, 80);
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
            var currentCard = sender as ToolCard;
            if (currentCard == null) return;
            var card = currentCard.DataContext as CardDataModel;
            if (card == null || vm == null || card.IsRenaming == true) return;
            e.Handled = true;//打断冒泡
            _DragStartPoint.Clear();
            _IsDragging = false;    
            _CurrentCard = currentCard;
            _IsCardLeftDown = true;//设置在卡片按下状态
            _CardStartPoint = e.GetPosition(ParamentCanvas);//记录鼠标位于画布上的位置       
            bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            //只有点击未选中的卡，且没按Ctrl，立刻清空
            if (!card.IsSelected && !isCtrl)
            {
                vm.ClearSeletionStatusCommand.Execute(null);
            }
            if (card.IsSelected && isCtrl)
            {
                vm.ClearSeletionStatusCommand.Execute(card);
            }
            else
            {
                vm.AddSeletionStatusCommand.Execute(card);//设置状态
            }
            var list = vm.AllCards.Where(c => c.IsSelected);
            foreach (var discard in list)
            {
                Point currentPoint = new Point(discard.X, discard.Y);
                _DragStartPoint[discard] = currentPoint;//按下control/准备拖拽时旧的状态不清且追加
            }
            _CurrentCard.CaptureMouse();//捕获鼠标
        }
        //Card上左键松开
        private void CardLeftUp(object sender, MouseButtonEventArgs e)
        {
            var currentCard = sender as ToolCard;
            if (currentCard == null) return;
            _IsCardLeftDown = false;//设置松开状态
            var card = currentCard.DataContext as CardDataModel;
            if (card == null || vm == null) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
            {
                if (_IsDragging == false)
                {
                    vm.ClearSeletionStatusCommand.Execute(null);//没按下control且不在拖拽时清除之前的卡片状态    
                    vm.AddSeletionStatusCommand.Execute(card);
                }
            }
            _IsDragging = false;//设置拖拽状态结束
            ParamentCanvas.Background = Brushes.Transparent;//设置网格
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
                _IsDragging = true; //超过阈值就设置移动状态
            }
            if (_IsDragging == false) return;
            ParamentCanvas.Background = FindResource("GridBrush") as Brush; ;//设置网格
            //此处添加移动卡片逻辑
            foreach (var card in _DragStartPoint)
            {
                var cardKey = card.Key;
                var cardPoint = card.Value;
                var newX = currentPoint.X - _CardStartPoint.X;
                var newY = currentPoint.Y - _CardStartPoint.Y;
                cardKey.X = Math.Round((card.Value.X + newX)/gridSize) * gridSize;
                cardKey.Y = Math.Round((card.Value.Y + newY) / gridSize) * gridSize;
            }
        }
        // ================= 画布平移 (右键拖拽) =================
        private void Canvas_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsRightDown = true;
            _hasPanned = false;

            // 记录鼠标在屏幕上的物理坐标
            _PanStartMousePos = e.GetPosition(this);
            // 记录此时画布的 Translate X 和 Y
            _PanStartTranslate = new Point(CanvasTranslate.X, CanvasTranslate.Y);

            var border = sender as Border;
            border?.CaptureMouse();
        }
        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_IsRightDown)
            {
                Point currentMousePos = e.GetPosition(this);
                double deltaX = currentMousePos.X - _PanStartMousePos.X;
                double deltaY = currentMousePos.Y - _PanStartMousePos.Y;
                if (Math.Abs(deltaX) >= DistanceThreshold || Math.Abs(deltaY) >= DistanceThreshold)
                {
                    _hasPanned = true;
                }
                if (_hasPanned)
                {
                    // 🌟 物理直觉：直接加上位移量！(向右拖鼠标，画布就向右走)
                    CanvasTranslate.X = _PanStartTranslate.X + deltaX;
                    CanvasTranslate.Y = _PanStartTranslate.Y + deltaY;
                }
            }
        }

        private void Canvas_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _IsRightDown = false;
            var border = sender as Border;
            border?.ReleaseMouseCapture();
            if (_hasPanned)
            {
                e.Handled = true; // 发生过拖拽，吃掉事件，不弹右键菜单
            }
            _hasPanned = false;
        }
        // ================= 画布缩放 (Ctrl + 滚轮) =================
        private void Canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                // 1. 获取鼠标在当前视口（Border）上的物理坐标
                var border = sender as Border;
                Point mousePos = e.GetPosition(border);

                // 2. 算好新的缩放比例
                double zoomStep = 0.1;
                double newScale = _currentZoom + (e.Delta > 0 ? zoomStep : -zoomStep);
                newScale = Math.Max(0.2, Math.Min(newScale, 5.0)); // 依然钳制在 0.2 到 5 之间
                if (newScale == _currentZoom) return; // 没变就退出
                // 🌟 3. 世界级图形学矩阵补偿算法（完美鼠标居中）
                // 公式：新的偏移 = 鼠标位置 - (鼠标位置 - 旧偏移) * (新缩放 / 旧缩放)
                double ratio = newScale / _currentZoom;
                CanvasTranslate.X = mousePos.X - (mousePos.X - CanvasTranslate.X) * ratio;
                CanvasTranslate.Y = mousePos.Y - (mousePos.Y - CanvasTranslate.Y) * ratio;
                // 4. 应用新的缩放比例
                CanvasScale.ScaleX = newScale;
                CanvasScale.ScaleY = newScale;
                _currentZoom = newScale;
                if (LeftZoomLevelText != null)
                {
                    LeftZoomLevelText.Text = $"{_currentZoom * 100:F0}%";
                }
            }
        }

        //==============================菜单/公共事件=======================
    }
}

