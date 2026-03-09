using System;
using System.Collections.Generic;
using System.Linq; // 提供 .ToList() 方法，防止集合在遍历时被修改导致崩溃
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using FirstVisionView.Card;
using WinRT; // 引用自定义卡片控件

namespace FirstVisionView
{
    /// <summary>
    /// 参数调整与视觉检测主界面后台逻辑
    /// </summary>
    public partial class AdjustPage : UserControl
    {
        public AdjustPage()
        {
            InitializeComponent();
        }

        // ============================== 1. 核心数据与状态字段 ===========================
        // [数据] 存储当前被选中的卡片实例（用于批量移动、删除）
        private List<ToolCard> SelectionCards = new List<ToolCard>();

        // [框选] 标记是否正在使用鼠标左键进行拉框
        private bool _isCanvasSelecting;

        // [框选] 记录拉框起始点（相对于逻辑画布 ParamentCanvas）
        private Point _SelectionStartPoint;

        // [拖拽] 标记是否正在拖拽卡片
        private bool _isDraggingCard;

        // [拖拽] 记录当前被直接点击的主卡片（领头羊）
        private ToolCard _CurrentClickCard;

        // [拖拽] 防手抖阈值（像素），鼠标移动超过此值才算真正拖拽
        private int Distance_Threshold = 5;

        // [拖拽] 记录拖拽那一瞬间鼠标的逻辑坐标
        private Point _DragStartMousePoint;

        // [拖拽] 记录拖拽开始时，所有选中卡片的原始绝对坐标（防叠罗汉核心）
        private Dictionary<ToolCard, Point> _DragStartPositions = new Dictionary<ToolCard, Point>();

        // [拖拽] 标记在按住期间是否真正发生了空间位移（用于决定松开时是否执行网格吸附）
        private bool _hasMoved;

        // ============================== 2. 画布缩放与平移字段 ===========================

        // [平移] 标记是否正在按住右键拖动画布背景
        private bool _isPanningCanvas;

        // [平移] 标记右键按下期间是否发生了拖拽（区分是“右键单击弹菜单”还是“右键拖动画布”）
        private bool _hasPanned;

        // [平移] 记录画布平移开始时的鼠标物理屏幕坐标（相对于 ScrollViewer）
        private Point _PanStartPoint;

        // [平移] 记录画布平移开始时，滚动条的水平初始位置
        private double _PanStartOffsetX;

        // [平移] 记录画布平移开始时，滚动条的垂直初始位置
        private double _PanStartOffsetY;

        // [缩放] 当前画布的缩放倍率（1.0 = 100%）
        private double _currentZoom = 1.0;

        // [缩放] 允许缩小的极限（0.02 = 2%）
        private double _minZoom = 0.02;

        // [缩放] 允许放大的极限（5.0 = 500%）
        private double _maxZoom = 3.0;

        // [缩放] 滚轮每滚一格的缩放系数（1.15代表每次变化15%）
        private double _zoomFactor = 1.15;

        // ============================== 3. 网格与智能布局参数 ===========================

        // 网格对齐颗粒度（像素）
        private double GridSize = 20;

        // 首张卡片生成的起始 X 坐标
        private double LayoutStartX = 20;

        // 首张卡片生成的起始 Y 坐标
        private double LayoutStartY = 20;

        // 自动排列时，卡片间的水平空隙
        private double LayoutSpacingX = 20;

        // 自动排列时，卡片间的垂直空隙
        private double LayoutSpacingY = 20;

        // 自动排列时，每行最多允许放几张卡片
        private int LayoutMaxColumns = 5;

        // 卡片尚未渲染完成时使用的默认估算宽度
        private double CardDefaultWidth = 150;

        // 卡片尚未渲染完成时使用的默认估算高度
        private double CardDefaultHeight = 80;

        // ============================== 4. 无限画布视图控制 (缩放与平移) ===========================

        /// <summary>
        /// 监听 ScrollViewer 滚轮事件，实现以鼠标为中心的矩阵缩放
        /// </summary>
        private void LeftScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 必须按住 Ctrl 键才允许缩放画布，否则执行默认的上下滚动
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)) return;

            // 拦截事件，防止触发默认滚动
            e.Handled = true;

            // 记录缩放前，鼠标在逻辑画布上的精确坐标（作为缩放的绝对中心锚点）
            Point mouseAtImage = e.GetPosition(LeftTransformGrid);

            // 根据滚轮方向更新缩放倍率
            if (e.Delta > 0) _currentZoom *= _zoomFactor; // 向上滚放大
            else _currentZoom /= _zoomFactor; // 向下滚缩小

            // 钳制缩放范围，防止画面无限大或缩小到看不见
            _currentZoom = Math.Max(_minZoom, Math.Min(_maxZoom, _currentZoom));

            // 将缩放值赋给 XAML 里的 ScaleTransform 对象
            LeftCanvasScale.ScaleX = _currentZoom;
            LeftCanvasScale.ScaleY = _currentZoom;

            // 强制 UI 线程立即重绘布局，使得后续滚动条获取的 Viewport 参数是最新的
            LeftTransformGrid.UpdateLayout();

            // 数学计算：计算使得鼠标指向的逻辑坐标点保持在屏幕原有物理位置所需的滚动条新偏移量
            double newOffsetX = (mouseAtImage.X * _currentZoom) - e.GetPosition(LeftScrollViewer).X;
            double newOffsetY = (mouseAtImage.Y * _currentZoom) - e.GetPosition(LeftScrollViewer).Y;

            // 应用新的滚动偏移
            LeftScrollViewer.ScrollToHorizontalOffset(newOffsetX);
            LeftScrollViewer.ScrollToVerticalOffset(newOffsetY);

            // 更新左下角的文本显示
            LeftZoomLevelText.Text = $"{(int)(_currentZoom * 100)}%";
        }

        /// <summary>
        /// 监听 ScrollViewer 右键按下，准备平移画布
        /// </summary>
        private void LeftScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 碰撞检测：如果鼠标当前指在某张卡片上，就放弃画布平移，把事件让给卡片去处理删除等操作
            if (e.OriginalSource is DependencyObject source && FindParent<ToolCard>(source) != null) return;

            // 确认点在空白处，初始化画布平移参数
            _isPanningCanvas = true;
            _hasPanned = false; // 重置平移标记

            // 记录鼠标的物理屏幕位置
            _PanStartPoint = e.GetPosition(LeftScrollViewer);

            // 记录当前滚动条的初始位置
            _PanStartOffsetX = LeftScrollViewer.HorizontalOffset;
            _PanStartOffsetY = LeftScrollViewer.VerticalOffset;

            // 鼠标指针变更为小手，提示用户可拖拽
            Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 监听 ScrollViewer 鼠标移动，执行画布平移
        /// </summary>
        private void LeftScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // 如果正在平移模式，且鼠标右键依然保持按下状态
            if (_isPanningCanvas && e.RightButton == MouseButtonState.Pressed)
            {
                // 获取当前鼠标物理位置
                Point currentPos = e.GetPosition(LeftScrollViewer);

                // 计算鼠标相对于按下的偏移量
                double dx = currentPos.X - _PanStartPoint.X;
                double dy = currentPos.Y - _PanStartPoint.Y;

                // 超过防抖阈值，认定为有效拖动画布
                if (Math.Abs(dx) > Distance_Threshold || Math.Abs(dy) > Distance_Threshold)
                {
                    _hasPanned = true; // 标记确实拖动了

                    // 核心算法：用滚动条的逆向移动来模拟画布的正向拖动
                    LeftScrollViewer.ScrollToHorizontalOffset(_PanStartOffsetX - dx);
                    LeftScrollViewer.ScrollToVerticalOffset(_PanStartOffsetY - dy);
                }
            }
        }

        /// <summary>
        /// 监听 ScrollViewer 右键抬起，结束平移并拦截菜单冲突
        /// </summary>
        private void LeftScrollViewer_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPanningCanvas)
            {
                // 退出平移模式
                _isPanningCanvas = false;

                // 恢复默认箭头指针
                Cursor = Cursors.Arrow;

                // 如果刚才确实拖动了画布，就吞掉这个事件，防止冒泡触发空白处的“添加卡片”菜单
                if (_hasPanned)
                {
                    e.Handled = true;
                }
                // 如果没拖动（短按），事件会自然向下传递给 Canvas_MouseRightButtonUp 弹出菜单
            }
        }

        // ============================== 5. 内部画布事件 (框选与右键添加) ===========================

        /// <summary>
        /// 画布左键按下：清理状态并开启拉框
        /// </summary>
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 如果没按 Ctrl，点击空白即视为放弃所有当前选择
            if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ClearAllSelection();
            }

            // 获取相对于逻辑画布的坐标作为起点
            _SelectionStartPoint = e.GetPosition(ParamentCanvas);

            // 开启框选模式
            _isCanvasSelecting = true;

            // 矩形长宽归零
            SelectionBox.Width = 0;
            SelectionBox.Height = 0;

            // 设置矩形初始位置
            Canvas.SetLeft(SelectionBox, _SelectionStartPoint.X);
            Canvas.SetTop(SelectionBox, _SelectionStartPoint.Y);

            // 显示半透明选框
            SelectionBox.Visibility = Visibility.Visible;

            // 画布捕获鼠标，确保框选滑出边界依然有效
            ParamentCanvas.CaptureMouse();
        }

        /// <summary>
        /// 画布左键抬起：结束框选
        /// </summary>
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 隐藏选框
            SelectionBox.Visibility = Visibility.Collapsed;

            // 退出框选模式
            _isCanvasSelecting = false;

            // 释放捕获
            ParamentCanvas.ReleaseMouseCapture();
        }

        /// <summary>
        /// 画布鼠标移动：刷新框选矩形并执行 AABB 碰撞检测
        /// </summary>
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isCanvasSelecting) return;

            // 获取当前鼠标逻辑坐标
            Point currentCanvasPoint = e.GetPosition(ParamentCanvas);

            // 智能计算矩形左上角（支持反向从右下往左上拉框）
            double boxX = Math.Min(_SelectionStartPoint.X, currentCanvasPoint.X);
            double boxY = Math.Min(_SelectionStartPoint.Y, currentCanvasPoint.Y);

            // 计算宽高绝对值
            double width = Math.Abs(currentCanvasPoint.X - _SelectionStartPoint.X);
            double height = Math.Abs(currentCanvasPoint.Y - _SelectionStartPoint.Y);

            // 更新 UI 选框属性
            Canvas.SetLeft(SelectionBox, boxX);
            Canvas.SetTop(SelectionBox, boxY);
            SelectionBox.Width = width;
            SelectionBox.Height = height;

            // 构造几何矩形用于碰撞计算
            Rect selectionRect = new Rect(boxX, boxY, width, height);

            // 遍历所有卡片
            foreach (var card in AllCards)
            {
                double cardLeft = Canvas.GetLeft(card);
                double cardTop = Canvas.GetTop(card);

                // 处理尚未被渲染引擎定位的新卡片引发的 NaN 问题
                if (double.IsNaN(cardLeft)) cardLeft = 0;
                if (double.IsNaN(cardTop)) cardTop = 0;

                // 构造卡片几何矩形（务必使用 ActualWidth/Height）
                Rect cardRect = new Rect(cardLeft, cardTop, card.ActualWidth, card.ActualHeight);

                // 碰撞检测
                if (selectionRect.IntersectsWith(cardRect))
                {
                    // 碰到了 -> 进列表并高亮
                    AddSelectionCard(card);
                    HighlightSingleCard(card);
                }
                else
                {
                    // 没碰到 -> 确保其不在选中列表中并恢复灰色
                    if (SelectionCards.Contains(card))
                    {
                        ResetCardEffect(card);
                        SelectionCards.Remove(card);
                    }
                }
            }
        }

        /// <summary>
        /// 画布右键抬起：短按空白处呼出添加菜单
        /// </summary>
        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 由于平移拖拽在 Preview 里被拦截了，能走到这里的只可能是空白处的纯粹短按
            // 如果没有选中的卡片，说明用户想新建卡片
            if (SelectionCards.Count == 0)
            {
                AddCard.IsOpen = true;
            }
            // 如果有选中的卡片，说明用户想删掉它们
            else
            {
                DeleteCard.IsOpen = true;
            }
        }

        // ============================== 6. 卡片交互区域 (拖拽、单/多选) =========================

        /// <summary>
        /// 卡片左键按下：处理选中状态、Ctrl组合键，并初始化拖拽锚点
        /// </summary>
        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var card = sender as ToolCard;
            if (card == null) return;

            // 检测 Ctrl 状态
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (isCtrlPressed)
            {
                // Ctrl + 点：追加选中
                AddSelectionCard(card);
                HighlightSingleCard(card);
            }
            else if (SelectionCards.Contains(card))
            {
                // 没按 Ctrl 但点了已选中的卡片：保留当前集合不变，准备集体拖拽
            }
            else
            {
                // 没按 Ctrl 点了新卡片：清场单选
                ClearAllSelection();
                AddSelectionCard(card);
                HighlightSingleCard(card);
            }

            // 更新主状态
            _CurrentClickCard = card;
            _isDraggingCard = true;
            _hasMoved = false;

            // 记录起点坐标

            _DragStartMousePoint = e.GetPosition(card);
            // 备份所有选中卡片坐标，以实现相对位移计算
            RecordDragStartPositions();

            // 吞掉事件，防止冒泡到 Canvas 触发清空
            e.Handled = true;

            // 卡片自身捕获鼠标，保障极端速度下的拖拽不丢帧
            card.CaptureMouse();
        }

        /// <summary>
        /// 卡片左键抬起：结束拖拽并触发智能吸附
        /// </summary>
        private void Card_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDraggingCard) return;

            // 关拖拽
            _isDraggingCard = false;

            // 释放焦点
            _CurrentClickCard?.ReleaseMouseCapture();

            // 只有真发生过位移，才需要去重新对齐网格
            if (_hasMoved)
            {
                SnapAllSelectedToGrid();
            }

            _hasMoved = false;
            e.Handled = true;
        }

        /// <summary>
        /// 卡片鼠标移动：多卡片同步绝对位移算法
        /// </summary>
        private void Card_MouseMove(object sender, MouseEventArgs e)
        {
            // 前置拦截
            if (!_isDraggingCard || _CurrentClickCard == null) return;

            e.Handled = true;

            // 实时逻辑坐标
            Point currentMousePoint = e.GetPosition(ParamentCanvas);

            // 算总差值 (Delta)
            double dx = currentMousePoint.X - _DragStartMousePoint.X;
            double dy = currentMousePoint.Y - _DragStartMousePoint.Y;

            // 过滤手抖
            if (Math.Abs(dx) <= Distance_Threshold && Math.Abs(dy) <= Distance_Threshold) return;

            _hasMoved = true;

            // 绝对位移算法：每个卡片的新位置 = 它的专属初始位置 + 鼠标总偏移量
            foreach (var card in SelectionCards)
            {
                if (_DragStartPositions.ContainsKey(card))
                {
                    var cardModel = card.DataContext as FirstVisionView.DataModel.CardDataModel;
                    if (cardModel != null)
                    {
                        Point originPos = _DragStartPositions[card]; 
                        cardModel.X = originPos.X + dx;
                        cardModel.Y = originPos.Y + dy;
                    }
                    
                }
            }
        }

        /// <summary>
        /// 卡片右键按下：支持 Ctrl剔除，或单选卡片
        /// </summary>
        private void Card_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true; // 拦截

            var card = sender as ToolCard;
            if (card == null) return;

            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (isCtrlPressed)
            {
                // Ctrl + 右键 = 精准剔除
                RemoveSelectionCard(card);
                if (SelectionCards.Count == 0) _CurrentClickCard = null;
                return; // 直接结束，不给后续弹菜单的机会
            }

            // 常规右键：如果已选中就无视，未选中就清场单选
            if (SelectionCards.Contains(card))
            {
                _CurrentClickCard = card;
            }
            else
            {
                ClearAllSelection();
                _CurrentClickCard = card;
                AddSelectionCard(card);
                HighlightSingleCard(card);
            }
        }

        /// <summary>
        /// 卡片右键抬起：呼出针对当前卡片的菜单
        /// </summary>
        private void Card_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            // 若在执行 Ctrl 剔除操作，别弹菜单烦人
            bool isCtrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (isCtrlPressed) return;

            if (SelectionCards.Count > 0)
            {
                DeleteCard.IsOpen = true; // 呼出删除菜单
            }
        }

        // ============================== 7. 菜单业务方法 ===========================

        /// <summary>
        /// 执行逻辑：生成卡片并智能摆放
        /// </summary>
        private void AddNewCard(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // 智能算法索要摆放坐标
            Point newPos = GetSmartNextPosition();

            // 实例化
            ToolCard newCard = new ToolCard();

            // 安置
            Canvas.SetLeft(newCard, newPos.X);
            Canvas.SetTop(newCard, newPos.Y);

            // 事件挂载
            newCard.MouseLeftButtonDown += Card_MouseLeftButtonDown;
            newCard.MouseLeftButtonUp += Card_MouseLeftButtonUp;
            newCard.MouseMove += Card_MouseMove;
            newCard.MouseRightButtonDown += Card_MouseRightButtonDown;
            newCard.MouseRightButtonUp += Card_MouseRightButtonUp;

            // 注入前端树和后端库
            ParamentCanvas.Children.Add(newCard);
            AllCards.Add(newCard);

            AddCard.IsOpen = false; // 关菜单
        }

        /// <summary>
        /// 执行逻辑：批量销毁选中卡片
        /// </summary>
        private void DeleCard(object sender, RoutedEventArgs e)
        {
            if (SelectionCards.Count == 0) return;

            // 【异常防范】遍历副本 ToList()，原集合随便删
            foreach (var card in SelectionCards.ToList())
            {
                ParamentCanvas.Children.Remove(card);
                AllCards.Remove(card);
            }

            // 打扫战场
            ClearAllSelection();
            DeleteCard.IsOpen = false;
        }

        // ============================== 8. 底层工具与排版算法 ===========================

        /// <summary>
        /// 工具：恢复单张卡片默认样式
        /// </summary>
        private void ResetCardEffect(ToolCard card)
        {
            if (card == null) return;
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 10,
                Color = (Color)ColorConverter.ConvertFromString("#20000000"),
                Opacity = 0.5,
                ShadowDepth = 1,
            };
            Canvas.SetZIndex(card, 0); // 降级层级
        }

        /// <summary>
        /// 工具：给单张卡片套上高亮发光壳
        /// </summary>
        private void HighlightSingleCard(ToolCard card)
        {
            if (card == null) return;
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 10,
                Color = Colors.DodgerBlue,
                Opacity = 0.8,
                ShadowDepth = 0
            };
            Canvas.SetZIndex(card, 99); // 提拔到最上层防止被遮挡
        }

        /// <summary>
        /// 宏观动作：一键清空所有业务和视觉选中状态
        /// </summary>
        private void ClearAllSelection()
        {
            foreach (var card in SelectionCards) ResetCardEffect(card);
            _CurrentClickCard?.ReleaseMouseCapture();
            _isDraggingCard = false;
            _hasMoved = false;
            _CurrentClickCard = null;
            SelectionCards.Clear();
            _DragStartPositions.Clear();
        }

        /// <summary>
        /// 工具：安全加入池子
        /// </summary>
        private void AddSelectionCard(ToolCard card)
        {
            if (card != null && !SelectionCards.Contains(card))
                SelectionCards.Add(card);
        }

        /// <summary>
        /// 工具：安全踢出池子并灭灯
        /// </summary>
        private void RemoveSelectionCard(ToolCard card)
        {
            if (card != null && SelectionCards.Contains(card))
            {
                ResetCardEffect(card);
                SelectionCards.Remove(card);
            }
        }

        /// <summary>
        /// 核心：快照抓取多张卡片的瞬间位置
        /// </summary>
        private void RecordDragStartPositions()
        {
            _DragStartPositions.Clear();
            foreach (var card in SelectionCards)
            {
                double left = Canvas.GetLeft(card);
                double top = Canvas.GetTop(card);
                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;
                _DragStartPositions[card] = new Point(left, top);
            }
        }

        /// <summary>
        /// 算法：数学四舍五入逼近最近网格线
        /// </summary>
        private double SnapToGrid(double value) => Math.Round(value / GridSize) * GridSize;

        /// <summary>
        /// 动作：强行把卡片拍到网格上
        /// </summary>
        private void SnapCardToGrid(ToolCard card)
        {
            if (card == null) return;
            double left = Canvas.GetLeft(card);
            double top = Canvas.GetTop(card);
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;
            Canvas.SetLeft(card, SnapToGrid(left));
            Canvas.SetTop(card, SnapToGrid(top));
        }

        /// <summary>
        /// 动作：全体列队
        /// </summary>
        private void SnapAllSelectedToGrid()
        {
            foreach (var card in SelectionCards) SnapCardToGrid(card);
        }

        /// <summary>
        /// 算法：寻址引擎。分析当前大盘分布，找出合理的下一个坑位
        /// </summary>
        private Point GetSmartNextPosition()
        {
            // 空盘直接发车点
            if (AllCards.Count == 0)
                return new Point(SnapToGrid(LayoutStartX), SnapToGrid(LayoutStartY));

            // 第一阶段：测探深渊（找 Y 轴最大值）
            double maxBottom = 0;
            foreach (var card in AllCards)
            {
                double top = Canvas.GetTop(card);
                if (double.IsNaN(top)) top = 0;
                double height = card.ActualHeight > 0 ? card.ActualHeight : CardDefaultHeight;
                if (top + height > maxBottom) maxBottom = top + height;
            }

            // 第二阶段：底边集结（捞出所有在渊底一线的兄弟）
            double tolerance = GridSize;
            List<ToolCard> bottomRowCards = new List<ToolCard>();
            foreach (var card in AllCards)
            {
                double top = Canvas.GetTop(card);
                if (double.IsNaN(top)) top = 0;
                double height = card.ActualHeight > 0 ? card.ActualHeight : CardDefaultHeight;
                // 允许容差范围内的算同行
                if (Math.Abs((top + height) - maxBottom) <= tolerance)
                    bottomRowCards.Add(card);
            }

            // 第三阶段：决定排兵布阵
            if (bottomRowCards.Count < LayoutMaxColumns)
            {
                // 横向扩展：这排还能塞，找这排最右边的
                double maxRight = 0;
                double rowTop = 0; // 同行 Y 轴复用
                foreach (var card in bottomRowCards)
                {
                    double left = Canvas.GetLeft(card);
                    double top = Canvas.GetTop(card);
                    if (double.IsNaN(left)) left = 0;
                    if (double.IsNaN(top)) top = 0;
                    double width = card.ActualWidth > 0 ? card.ActualWidth : CardDefaultWidth;

                    if (left + width > maxRight)
                    {
                        maxRight = left + width;
                        rowTop = top;
                    }
                }
                // 紧贴其后加间距
                return new Point(SnapToGrid(maxRight + LayoutSpacingX), SnapToGrid(rowTop));
            }
            else
            {
                // 纵向深掘：满了，换行，顶头从零开始，深度下降
                return new Point(SnapToGrid(LayoutStartX), SnapToGrid(maxBottom + LayoutSpacingY));
            }
        }

        /// <summary>
        /// 泛型辅助：顺着逻辑树往上爬，找出特定的控件父级（用于解决事件源头判断）
        /// </summary>
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }
    }
}