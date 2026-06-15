using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VisionView.DataModel;

namespace VisionView.UIControls // 导师提示：注意这里的命名空间
{
    // ==========================================
    // 核心职责：完全接管连线的底层 GPU 渲染与数学射线点击检测
    // 第一性原理：绝不使用臃肿的 UIElement，直接用 DrawingContext 涂鸦
    // ==========================================
    public class LightWireCanvas : FrameworkElement
    {
        // 智能快递柜：接收来自 ViewModel 的 AllWires 数据
        public static readonly DependencyProperty WiresSourceProperty =
            DependencyProperty.Register("WiresSource", typeof(IEnumerable<WireDataModel>), typeof(LightWireCanvas),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnWiresSourceChanged));

        public IEnumerable<WireDataModel> WiresSource
        {
            get { return (IEnumerable<WireDataModel>)GetValue(WiresSourceProperty); }
            set { SetValue(WiresSourceProperty, value); }
        }

        // 快递柜报警器：数据变化时，强迫画面重新渲染
        private static void OnWiresSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LightWireCanvas)d).InvalidateVisual();
        }

        private readonly Pen _defaultPen;
        private readonly Pen _selectedPen;
        private readonly Pen _hitTestPen; // 用于碰撞检测的隐形粗笔

        public LightWireCanvas()
        {
            // Freeze() 冻结画笔，节省极大的内存开销
            _defaultPen = new Pen(Brushes.Gray, 3.0);
            _defaultPen.Freeze();

            _selectedPen = new Pen(Brushes.DodgerBlue, 3.0);
            _selectedPen.Freeze();

            // 造一支 15 像素宽的隐形笔，容错率极高，方便鼠标点中 3 像素的线
            _hitTestPen = new Pen(Brushes.Transparent, 15.0);
            _hitTestPen.Freeze();

            ClipToBounds = false; // 允许画到边界外
        }

        // ==========================================
        // 核心动作一：老画师无情作画 (极速渲染)
        // ==========================================
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (WiresSource == null) return;

            foreach (var wire in WiresSource)
            {
                if (string.IsNullOrEmpty(wire.PathData)) continue;

                // 将 SVG 字符串转为显卡认识的几何图形
                Geometry geo = Geometry.Parse(wire.PathData);
                Pen penToUse = wire.IsSelected ? _selectedPen : _defaultPen;

                // 向 DirectX 发送绘制指令
                dc.DrawGeometry(null, penToUse, geo);
            }
        }

        // ==========================================
        // 核心动作二：隔空点穴 (数学射线 HitTest)
        // ==========================================
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (WiresSource == null) return;

            Point mousePos = e.GetPosition(this);
            WireDataModel clickedWire = null;

            // 倒序遍历：后画的线在最上面，优先被点中
            var wiresList = new List<WireDataModel>(WiresSource);
            for (int i = wiresList.Count - 1; i >= 0; i--)
            {
                var wire = wiresList[i];
                if (string.IsNullOrEmpty(wire.PathData)) continue;

                Geometry geo = Geometry.Parse(wire.PathData);

                // 物理隐喻：用 15 像素的隐形刷子扫一下这条线，看鼠标在不在刷子扫过的痕迹里
                if (geo.StrokeContains(_hitTestPen, mousePos))
                {
                    clickedWire = wire;
                    break;
                }
            }

            // 如果点中了某根线
            if (clickedWire != null)
            {
                e.Handled = true; // 拦截事件，不让 Canvas 觉得点到了空白处

                // 找到所有的线，把选中状态清空
                foreach (var w in wiresList) w.IsSelected = false;

                // 点亮这根线
                clickedWire.IsSelected = true;

                // 呼叫老画师重新画一遍
                this.InvalidateVisual();
            }
            else
            {
                // 如果点到了空白处，清空所有线的选中状态
                foreach (var w in wiresList) w.IsSelected = false;
                this.InvalidateVisual();
            }

            base.OnMouseLeftButtonDown(e);
        }
    }
}