using System.Collections.Specialized;
using System.ComponentModel;
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
            var canvas = (LightWireCanvas)d;
            // 防呆设计：如果拆除旧信箱，必须把上面的报警线全部剪断，否则会导致内存泄漏 (Memory Leak)！
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= canvas.OnCollectionChanged;
                // 把旧信箱里每一封信身上的追踪器也拆了
                foreach (var item in (IEnumerable<WireDataModel>)e.OldValue)
                {
                    item.PropertyChanged -= canvas.OnWirePropertyChanged;
                }
            }
            // 安装新信箱的报警线
            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += canvas.OnCollectionChanged;
                // 给新信箱里现存的信件挨个装上追踪器
                foreach (var item in (IEnumerable<WireDataModel>)e.NewValue)
                {
                    item.PropertyChanged += canvas.OnWirePropertyChanged;
                }
            }

            canvas.InvalidateVisual(); // 强制画一笔
        }
        // 2. 信箱里多了一封信（连新线）或少了一封信（删除线）时的处理逻辑
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 有新线装上属性变动
            if (e.NewItems != null)
            {
                foreach (WireDataModel newItem in e.NewItems)
                    newItem.PropertyChanged += OnWirePropertyChanged;
            }

            // 有线被删了释放内存
            if (e.OldItems != null)
            {
                foreach (WireDataModel oldItem in e.OldItems)
                    oldItem.PropertyChanged -= OnWirePropertyChanged;
            }

            this.InvalidateVisual(); // 集合变了重画
        }

        // 3.卡片被拖动导致 PathData 变了，或被选中了
        private void OnWirePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 如果是 PathData（坐标改变）或 IsSelected（颜色改变），直接重画
            this.InvalidateVisual();
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
        // 渲染
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
        // 数学射线 HitTest
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

                // 用 15 像素的隐形刷子扫一下这条线，看鼠标在不在刷子扫过的痕迹里
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

                // 重新画一遍
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