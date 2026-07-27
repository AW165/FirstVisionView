namespace VisionView.HardWare
{
    public class CameraManager
    {
        //
        private List<ICameraDriver> _drivers = new List<ICameraDriver>();//存储所有相机驱动的列表
        public void LoadDrivers()
        {
            //加载相机驱动
            _drivers.Add(new HikDriver());
        }
        public List<CameraInfo> GetAllCameras()
        {
            List<CameraInfo> allCams = new();//新建一个空的列表来存储所有相机信息
            foreach (var driver in _drivers)
            {
                allCams.AddRange(driver.SearchCameras());
            }
            return allCams;
        }
    }
}
