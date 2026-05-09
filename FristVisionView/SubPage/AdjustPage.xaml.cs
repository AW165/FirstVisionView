// 引入系统基础类库，提供基础数据类型和底层运算支持
// 引入泛型集合库，为我们提供 Dictionary（字典）等高级数据结构
// 引入 LINQ 查询语言，让我们能像查数据库一样查内存里的集合（比如用 Where, Any）
// 引入 WPF 核心基础库，提供 Point（坐标点）、Rect（矩形）、依赖属性等基础对象
using System.Windows;
// 引入 WPF 控件库，提供 UserControl（用户控件）、Canvas（画布）等 UI 元素
using System.Windows.Controls;
// 引入 WPF 输入控制库，提供鼠标事件（MouseEventArgs）、键盘按键（Key）的监听能力
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Messaging;
using FirstVisionView.Card;
// 引入自己的数据模型库，拿到 CardDataModel
using FirstVisionView.DataModel;
using FirstVisionView.ParameterUILibary.ParameterModel;

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
            WeakReferenceMessenger.Default.Register<string>(this, (recipient, message) =>
            {
                // 核对
                if (message == "ImageSelected")
                {
                    // 延时
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SelfAdaption(); // 执行居中逻辑
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
            });
        }
        //引用ViewModel，拿到vm的数据
        private double gridSize = 10;
        private AdjustViewModel? vm => this.DataContext as AdjustViewModel; //vm模型
        //画框状态
        private bool _IsDragging = false;
        //鼠标左键在Canvas按下状态
        private bool _IsCanvasLeftDown = false;
        //鼠标左键在Card按下状态
        private bool _IsCardLeftDown = false;
        //鼠标右键按下状态
        private bool _IsRightDown = false;
        //记录鼠标点击Canvas时的当前坐标
        private Point _CanvasStartPoint;
        //记录鼠标点击Card时的当前坐标
        private Point _CardStartPoint;
        //记录鼠标点击Card
        private ToolCard _CurrentCard;
        //记录卡片与坐标点
        private Dictionary<CardDataModel, Point> _DragStartPoint = new();
        // 标记在右键按下期间，是否真正发生了拖拽动作（用于区分“右键呼出菜单”和“右键拖动画布”）。
        private bool _hasPanned;
        // 记录当前整个大画布的缩放倍率，默认 1.0 代表 100% 原始大小。
        private double _currentZoom = 1.0;
        private double _imageZoom = 1.0;
        private const int DistanceThreshold = 5;// 防手抖的像素阈值：鼠标按下后移动超过 5 个像素，才被正式认定为“拖拽行为”，否则视为原地点击。
        private Point _PanStartMousePos;// 记录鼠标在屏幕上的物理坐标
        private Point _PanStartTranslate; // 记录按下时，画布原本的偏移量
        private bool _IsDringGhostLine = false;//记录连线是否在拖动
        private string _StartPinDirection;//记录从哪个方向出来的线段
        private Point _LineStartPoint;//记录点击的坐标
        private ToolCard _LineStartCard;//记录是点击的是哪个卡片的pin
        private bool _hasClickImage;//记录是否拖动过图片
        private Point _OldImagePoint;//记录图片点位


        //============================Canvas事件区域======================
        //Canvas上左键点击
        private void CanvasLeftDown(object sender, MouseButtonEventArgs e)
        {
            ParamentCanvas.Focus();
            _IsCanvasLeftDown = true;//设置左键点击状态
            _DragStartPoint.Clear();//清空所有的选择卡片
            if (vm == null) return;//如果没持有vm则直接退出
            bool isControl = ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);//检查有无按下control键
            if (!isControl)
            {
                vm.ClearSeletionStatusCommand.Execute(null);//清除卡片高亮状态
                vm.WireCleanStatusCommand.Execute(null);//清除线段高亮状态
            }
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
            if (_IsDringGhostLine)
            {
                //  停止画线，虚线
                _IsDringGhostLine = false;
                GhostLine.Visibility = Visibility.Collapsed;
                //  拿到鼠标松开时的绝对位置
                Point dropPoint = e.GetPosition(ParamentCanvas);
                // 向下穿透刺探：鼠标底下有没有扎到控件
                Ellipse targetPin = null;
                VisualTreeHelper.HitTest(
                    ParamentCanvas,
                    null,
                    new HitTestResultCallback(result =>
                    {
                        if (result.VisualHit is Ellipse pin && pin.Name.EndsWith("Pin"))
                        {
                            targetPin = pin;
                            return HitTestResultBehavior.Stop;
                        }
                        return HitTestResultBehavior.Continue;
                    }),
                    new GeometryHitTestParameters(new EllipseGeometry(dropPoint, 15, 15))
                    );


                // 如果扎到了，而且扎到的是一个叫 Ellipse 的圆孔
                if (targetPin != null)
                {
                    string targetPinName = targetPin.Name;
                    ToolCard targetCard = FindParent<ToolCard>(targetPin);
                    // 防呆设计：不能自己连自己，也不能连到没名字的孔上
                    if (string.IsNullOrEmpty(targetPinName) || dropPoint == _LineStartPoint) return;
                    // 4. 获取目标孔所在的卡片
                    if (targetCard != null && targetCard != _LineStartCard)
                    {
                        // 获取卡片的业务数据
                        var targetCardData = targetCard.DataContext as CardDataModel;
                        if (targetCardData == null) return;
                        // 5. 获取目标孔的绝对坐标（为了让线精准对准中心）
                        Point targetPinCenter = new Point(targetPin.Width / 2, targetPin.Height / 2);
                        Point absoluteEndPoint = targetPin.TransformToAncestor(ParamentCanvas).Transform(targetPinCenter);
                        var startCard = _LineStartCard.DataContext as CardDataModel;//转换成M，为创建wireM做准备
                        if (startCard == null) return;
                        //  6. 生成一根永久的实体连线数据！
                        WireDataModel newWire = new WireDataModel()
                        {
                            StartPoint = _LineStartPoint,//从哪个点开始的
                            EndPoint = absoluteEndPoint,//到哪个点结束
                            SourceCard = startCard,//从哪个卡片开始的
                            TargetCard = targetCardData,//到哪个卡片结束
                            SourcePin = _StartPinDirection,
                            EndPin = targetPinName
                        };
                        newWire.UpdatePath(); // 让线自己计算一下贝塞尔曲线形状
                        // 7. 塞进大账本！UI 瞬间渲染出这根实线！
                        if (vm != null)
                        {
                            vm.AllWires.Add(newWire);
                        }

                    }
                }
            }
            GhostLine.Data = null;//清空虚线
        }
        //Canvas上右键松开
        private void RightUp(object sender, MouseButtonEventArgs e)
        {
            _IsRightDown = false;//取消右键按下状态
            _IsDragging = false;//取消拖拽状态
            if (vm == null) return;
            var cardList = vm.AllCards.Where(c => c.IsSelected).ToList();//判断有无卡片被选中，有则弹出删除菜单，否则弹出添加菜单
            var wiredList = vm.AllWires.Where(c => c.IsSelected).ToList();//判断有无线段被选中，有则弹出删除菜单，否则弹出添加菜单
            if (cardList.Count == 0 && wiredList.Count == 0)
            {
                Point logicalPos = e.GetPosition(ParamentCanvas);
                vm.CurrentMousePoint = new System.Drawing.PointF((float)logicalPos.X, (float)logicalPos.Y); // 假设你在 ViewModel 里用 PointF 或者 WPF的Point 存它
                return;
            }
            else vm.DelePopup = true;
        }
        //Canvas上鼠标移动
        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            var currentPoint = e.GetPosition(ParamentCanvas);
            if (_IsDringGhostLine)
            {
                double tension = 50;
                Point controlPoint1 = _LineStartPoint;
                Point controlPoint2 = currentPoint;
                switch (_StartPinDirection)
                {
                    case "RightPin": controlPoint1.X += tension; break;
                    case "LeftPin": controlPoint1.X -= tension; break;
                    case "TopPin": controlPoint1.Y -= tension; break;
                    case "BottomPin": controlPoint1.Y += tension; break;
                }
                // 终点磁铁暂时假定相对水平或垂直扎入
                if (_StartPinDirection == "RightPin" || _StartPinDirection == "LeftPin")
                    controlPoint2.X -= tension;
                else
                    controlPoint2.Y -= tension;

                // 💥 拼装 SVG 路径字符串，画出贝塞尔曲线
                string pathData = $"M {_LineStartPoint.X},{_LineStartPoint.Y} " +
                                  $"C {controlPoint1.X},{controlPoint1.Y} " +
                                  $"{controlPoint2.X},{controlPoint2.Y} " +
                                  $"{currentPoint.X},{currentPoint.Y}";

                // 解析并赋值给 XAML 里的 GhostLine 控件
                GhostLine.Data = Geometry.Parse(pathData);
                // ⚠️ 极其致命的一步：正在画线，直接强行退出方法！绝不允许执行下面的框选逻辑！
                return;
            }//画线逻辑
            if (_IsCanvasLeftDown != true) return;//左键未按下则返回
            var disX = Math.Abs(currentPoint.X - _CanvasStartPoint.X);
            var disY = Math.Abs(currentPoint.Y - _CanvasStartPoint.Y);
            if (disX >= DistanceThreshold || disY >= DistanceThreshold)
            {
                _hasPanned = true; //超过阈值就设置移动状态
            }
            if (_hasPanned == false) return;//没触发拖拽就返回
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
            var currentCardModel = currentCard.DataContext as CardDataModel;
            if (currentCardModel != null && currentCardModel.ParameterVM != null)
            {
                if (currentCardModel.ParameterVM.GetType() == typeof(ImageProvider))
                {
                    vm.IsImagesourceVisible = true;
                }
                else
                {
                    vm.IsImagesourceVisible = false;
                }
            }
            vm.WireCleanStatusCommand.Execute(null);//清除所有线段的选中状态
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
            if (_IsCardLeftDown == false) return;
            GridBackgroundLayer.Visibility = Visibility.Collapsed;//设置显示网格
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
            GridBackgroundLayer.Visibility = Visibility.Visible;//设置显示网格
            //此处添加移动卡片逻辑
            foreach (var card in _DragStartPoint)
            {
                var cardKey = card.Key;
                var cardPoint = card.Value;
                var newX = currentPoint.X - _CardStartPoint.X;
                var newY = currentPoint.Y - _CardStartPoint.Y;
                var frameX = Math.Round((card.Value.X + newX) / gridSize) * gridSize;
                var frameY = Math.Round((card.Value.Y + newY) / gridSize) * gridSize;
                var differX = frameX - cardKey.X;
                var differY = frameY - cardKey.Y;
                cardKey.X = frameX;
                cardKey.Y = frameY;
                if (differX != 0 || differY != 0)
                {
                    foreach (var wire in vm.AllWires)
                    {
                        if (wire.SourceCard == cardKey || wire.TargetCard == cardKey)
                        {
                            if (wire.SourceCard == cardKey)
                            {
                                wire.StartPoint = new Point(wire.StartPoint.X + differX, wire.StartPoint.Y + differY);
                            }
                            if (wire.TargetCard == cardKey)
                            {
                                wire.EndPoint = new Point(wire.EndPoint.X + differX, wire.EndPoint.Y + differY);
                            }
                            wire.UpdatePath();
                        }

                    }
                }
            }
        }
        /// <summary>
        /// 双击卡片后，读取点击的卡片Vm，并填充到CurrentEditVm中，打开参数修改卡片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CardDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (e.OriginalSource is Ellipse) return;
            e.Handled = true;
            if (_CurrentCard != null)
            {
                var ToolCardVM = sender as ToolCard;
                if (ToolCardVM == null) return;
                var CardVM = ToolCardVM.DataContext as CardDataModel;
                if (CardVM == null || vm == null) return;
                vm.CurrentEditVM = CardVM.ParameterVM;
                vm.Cap = true;
            }

        }
        // ================= 画布平移 (右键拖拽) =================
        private void CanvasRightDown(object sender, MouseButtonEventArgs e)
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
        //开始画线
        public void StartDrawingLine(Point startPoint, string pinName, ToolCard card)
        {
            _LineStartCard = card;//记录当前点击的卡片，用于
            _IsDringGhostLine = true;//开启画线状态
            _LineStartPoint = startPoint;
            _StartPinDirection = pinName;
            GhostLine.Data = null;//删除之前的虚线
            GhostLine.Visibility = Visibility.Visible;//开启虚线可视化
        }
        //==============================线段=======================
        private void WireLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var path = sender as Path;
            if (path == null) return;
            var wire = path.DataContext as WireDataModel;
            if (wire == null || vm == null) return;
            vm.ClearSeletionStatusCommand.Execute(null);
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                vm.WireAddStatusCommand.Execute(wire);
            }
            else
            {
                vm.WireCleanStatusCommand.Execute(null);
                vm.WireAddStatusCommand.Execute(wire);
            }
        }
        //==============================工具=======================
        //寻找传入控件的父关系控件
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            if (parent != null) return parent;
            else return FindParent<T>(parentObject);
        }

        private void ItemsControl_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {

        }

        private void imgDisplay_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            // 1. 获取鼠标在当前视口（Grid）上的物理坐标
            Point mousePos = e.GetPosition(ImageContainer);
            double zoomStep = 0.2;

            // 2. 算好新的缩放比例
            if (_imageZoom <= 4)
            {
                zoomStep = 0.2;
            }
            else zoomStep = 6;
            double newScale = _imageZoom + (e.Delta > 0 ? zoomStep : -zoomStep);
            newScale = Math.Max(0.2, Math.Min(newScale, 80.0)); // 依然钳制在 0.2 到 70 之间
            if (newScale == _imageZoom) return; // 没变就退出

            if (newScale >= 40) ImageBackgroundLayer.Visibility = Visibility.Visible; else ImageBackgroundLayer.Visibility = Visibility.Collapsed;                                // 公式：新的偏移 = 鼠标位置 - (鼠标位置 - 旧偏移) * (新缩放 / 旧缩放)
            double ratio = newScale / _imageZoom;
            ImageCanvasTranslate.X = mousePos.X - (mousePos.X - ImageCanvasTranslate.X) * ratio;
            ImageCanvasTranslate.Y = mousePos.Y - (mousePos.Y - ImageCanvasTranslate.Y) * ratio;
            // 4. 应用新的缩放比例
            ImageCanvasScale.ScaleX = newScale;
            ImageCanvasScale.ScaleY = newScale;
            _imageZoom = newScale;
        }
        //图片左键按下
        private void ImageLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var image = sender as Image;
            if (image == null || vm == null) return;
            _OldImagePoint = e.GetPosition(ImageContainer);
            _hasClickImage = true;
            imgDisplay.CaptureMouse();
        }
        //图片左键松开
        private void ImageLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var image = sender as Image;
            if (image == null || vm == null) return;
            _OldImagePoint = e.GetPosition(ImageContainer);
            _hasClickImage = false;
            imgDisplay.ReleaseMouseCapture();
        }
        //图片缩放
        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            if (_hasClickImage)
            {
                var image = sender as Image;
                if (image == null || vm == null) return;
                var currentPoint = e.GetPosition(ImageContainer);
                var deltaX = currentPoint.X - _OldImagePoint.X;
                var deltaY = currentPoint.Y - _OldImagePoint.Y;
                ImageCanvasTranslate.X += deltaX;
                ImageCanvasTranslate.Y += deltaY;
                _OldImagePoint = currentPoint;
            }
        }
        private void FullButtonClick(object sender, RoutedEventArgs e)
        {
            SelfAdaption();

        }
        private void SelfAdaption()
        {
            if (vm == null) return;
            var imgWidth = imgDisplay.ActualWidth;
            var imgHeight = imgDisplay.ActualHeight;
            var gridWidth = ImageContainer.ActualWidth;
            var gridHeight = ImageContainer.ActualHeight;
            var coifficientX = gridWidth / imgWidth;
            var coifficientY = gridHeight / imgHeight;
            var ratio = Math.Min(coifficientY, coifficientX);
            ImageCanvasScale.ScaleX = ratio;
            ImageCanvasScale.ScaleY = ratio;
            _imageZoom = ratio;
            imgWidth = imgDisplay.Source.Width * ratio;
            imgHeight = imgDisplay.Source.Height * ratio;
            ImageCanvasTranslate.X = (gridWidth - imgWidth) / 2;
            ImageCanvasTranslate.Y = (gridHeight - imgHeight) / 2;

        }

        //==============================菜单/公共事件=======================
    }
}

