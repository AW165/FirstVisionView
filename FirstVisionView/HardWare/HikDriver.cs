namespace VisionView.HardWare
{
    public class HikDriver : ICameraDriver
    {
        //相机名称
        public string DriverName => "HikDriver";
        //相机信息合集
        public List<CameraInfo> SearchCameras()
        {
            return new List<CameraInfo>();
        }
        public ICamera CreateCamera(CameraInfo Info)
        {
            return null;
        }

    }
}
