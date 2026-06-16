using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VisionView
{
    public class IndustrialImageViewer : FrameworkElement
    {
        // 矩阵状态机（外部控制镜头的推拉摇移）
        public double Zoom { get; private set; } = 1.0;
        public double OffsetX { get; private set; } = 0.0;
        public double OffsetY { get; private set; } = 0.0;

        // 🌟 核心器官 1：底层数据源
        private BitmapSource _sourceBitmap; // 扔给显卡用的“底片”
        private byte[] _pixelBuffer;        // 留在内存里给自己取色用的“血库”
        private int _pixelWidth;
        private int _pixelHeight;
        private int _stride;                // 一行像素占用的字节数
        public int ImageWidth => _pixelWidth;
        public int ImageHeight => _pixelHeight;
        public bool HasImage => _sourceBitmap != null;
        public IndustrialImageViewer()
        {
            // ⚠️ 导师补习班：关闭 WPF 默认的双线性插值。
            // 这行代码是机器视觉的尊严！它命令显卡：“放大时绝对不准模糊化，必须保持像素的方块感！”
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            ClipToBounds = true;
        }

        // ==========================================
        // 动作 1：装载数据（只在切图时执行一次，O(1)复杂度）
        // ==========================================
        public void SetSource(BitmapSource source)
        {
            if (source == null)
            {
                _sourceBitmap = null;
                _pixelBuffer = null;
                _pixelWidth = 0;
                _pixelHeight = 0;
                _stride = 0;
                InvalidateVisual();
                return;
            }

            // 强制统一格式为 Bgra32 (蓝绿红透明度)，这样每个像素固定占 4 个字节，方便我们心算找位置
            var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            converted.Freeze(); // 彻底冻结这块底片

            _sourceBitmap = converted;
            _pixelWidth = converted.PixelWidth;
            _pixelHeight = converted.PixelHeight;
            _stride = _pixelWidth * 4;//每行像素占用的字节数 = 图片宽度 * 每个像素占用的字节数（4）

            // 抽血：把所有数字提前吸进内存，剥离对 UI 的依赖
            _pixelBuffer = new byte[_pixelHeight * _stride];//构建矩形库存放像素数据
            converted.CopyPixels(_pixelBuffer, _stride, 0);//拷贝像素数据到内存中，0 是偏移量，表示从数组的开头开始写入，_stride 是每行像素占用的字节数，确保正确地排列像素数据
            // 重新刷新画面
            InvalidateVisual();
        }

        // ==========================================
        // 动作 2：更新矩阵（推拉摇移）
        // ==========================================
        public void UpdateTransform(double zoom, double offsetX, double offsetY)
        {
            Zoom = zoom;//缩放倍率
            OffsetX = offsetX;//平移 X
            OffsetY = offsetY;//平移 Y
            InvalidateVisual(); // 触发 OnRender
        }

        // ==========================================
        // 动作 3：终极渲染通道（每秒 60 帧极速运行）
        // ==========================================
        protected override void OnRender(DrawingContext dc)
        {
            // 1. 铺实心底板（建立拦截鼠标的物理墙）
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30)), null, new Rect(0, 0, ActualWidth, ActualHeight));
            //actualWidth 和 ActualHeight 是 WPF 提供的属性表示当前控件在屏幕上实际占用的宽度和高度。
            //通过使用这些属性，我们可以确保绘制的背景矩形完全覆盖整个控件区域，无论控件的大小如何变化。
            if (_sourceBitmap == null) return;

            // 🌟 2. 核心大招：让 GPU 去画 5000 万像素！
            // 算出这张底片在当前缩放和平移下，应该在屏幕的什么物理位置
            Rect imageRect = new Rect(OffsetX, OffsetY, _pixelWidth * Zoom, _pixelHeight * Zoom);
            dc.DrawImage(_sourceBitmap, imageRect);// 这行代码是整个工业级图像查看器的核心,它命令 GPU“把这张底片按照这个矩阵变换，迅速画到屏幕上！”

            // 🌟 3. 动态画网格（当放大超过 20 倍时）
            if (Zoom >= 20)
            {
                double viewW = ActualWidth;
                double viewH = ActualHeight;

                // 这就是你觉得懵的公式：其实就是算“屏幕最左边和最右边，压在图片的哪个坐标上”
                int startX = (int)Math.Max(0, Math.Floor(-OffsetX / Zoom));
                int startY = (int)Math.Max(0, Math.Floor(-OffsetY / Zoom));
                int endX = (int)Math.Min(_pixelWidth, Math.Ceiling((viewW - OffsetX) / Zoom));
                int endY = (int)Math.Min(_pixelHeight, Math.Ceiling((viewH - OffsetY) / Zoom));

                StreamGeometry gridGeo = new StreamGeometry();// 轻量级的几何图形容器，适合存储大量简单线段，性能比 PathGeometry 更好
                using (StreamGeometryContext ctx = gridGeo.Open())//using 语句确保在绘制完成后正确释放资源,StreamGeometryContext 是 StreamGeometry 的上下文对象，提供了一系列方法来定义几何图形的路径
                {
                    // 画竖线
                    for (int x = startX; x <= endX; x++)
                    {
                        double screenX = x * Zoom + OffsetX;
                        ctx.BeginFigure(new Point(screenX, startY * Zoom + OffsetY), false, false);
                        ctx.LineTo(new Point(screenX, endY * Zoom + OffsetY), true, false);
                    }
                    // 画横线
                    for (int y = startY; y <= endY; y++)
                    {
                        double screenY = y * Zoom + OffsetY;
                        ctx.BeginFigure(new Point(startX * Zoom + OffsetX, screenY), false, false);
                        ctx.LineTo(new Point(endX * Zoom + OffsetX, screenY), true, false);
                    }
                }
                gridGeo.Freeze(); // 冻结这一帧的几十根网格线，免得 GC 报错

                // 网格线的粗细永远是 1 绝对物理像素，颜色半透明灰
                Pen gridPen = new Pen(new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0xFF, 0xFF)), 1.0);
                gridPen.Freeze();

                dc.DrawGeometry(null, gridPen, gridGeo);//null 表示不填充，gridPen 是用来描边的画笔，gridGeo 是我们定义的网格线几何图形，这行代码命令 GPU“把这些线按照这个矩阵变换，迅速画到屏幕上”
            }
        }

        // ==========================================
        // 动作 4：光速取色 (O(1) 复杂度)
        // ==========================================
        public bool TryGetPixelAt(Point screenPoint, out byte r, out byte g, out byte b)
        {
            r = g = b = 0;
            if (_pixelBuffer == null) return false;

            // ⚠️ 导师补习班：物理坐标反算
            // 鼠标点在屏幕 800 的位置，减去平移 OffsetX，除以放大 Zoom，瞬间算出点在数组的第几行第几列
            int pixelX = (int)Math.Floor((screenPoint.X - OffsetX) / Zoom);
            int pixelY = (int)Math.Floor((screenPoint.Y - OffsetY) / Zoom);

            // 越界保护
            if (pixelX < 0 || pixelX >= _pixelWidth || pixelY < 0 || pixelY >= _pixelHeight)
                return false;

            int idx = pixelY * _stride + pixelX * 4;//每行占用的字节数乘以行数，再加上每个像素占用的字节数乘以列数，算出这个像素在数组中的起始位置
            b = _pixelBuffer[idx + 0]; // Blue
            g = _pixelBuffer[idx + 1]; // Green
            r = _pixelBuffer[idx + 2]; // Red
            return true;
        }
    }
}