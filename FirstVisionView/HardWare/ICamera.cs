using System.Windows.Media.Imaging;

namespace VisionView.HardWare
{
    /// <summary>
    /// 相机通用接口，定义了相机的基本操作方法，包括打开、关闭和抓取图像。
    /// </summary>
    public interface ICamera
    {
        void Open();
        void Close();
        BitmapSource GrabOne();
    }
    public interface ICameraDriver
    {

        string DriverName { get; }
        //相机信息合集
        List<CameraInfo> SearchCameras();
        //创建相机实例
        ICamera CreateCamera(CameraInfo Info);
    }
    public class CameraInfo
    {
        public string Name { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string DriverName { get; set; } = "Unknown";
    }
}
