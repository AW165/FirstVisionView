using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MvCamCtrl.NET;

namespace VisionView.HardWare
{
    /// <summary>
    /// 海康相机驱动，只负责枚举、打开和单张拍照取图。
    /// </summary>
    public class HikCamera : ICamera, IDisposable
    {
        private const int GrabTimeoutMs = 3000;
        private readonly object _syncRoot = new();
        private readonly CameraInfo? _info;
        private MyCamera? _camera;
        private bool _isOpen;
        private bool _isGrabbing;

        public HikCamera(CameraInfo? info)
        {
            _info = info;
        }

        /// <summary>
        /// 枚举当前电脑上的海康 GigE/USB 相机。
        /// </summary>
        public static List<CameraInfo> EnumerateCameras()
        {
            var deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            int result = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);
            EnsureSdkSuccess("枚举海康相机", result);

            var cameras = new List<CameraInfo>();
            for (int index = 0; index < deviceList.nDeviceNum; index++)
            {
                MyCamera.MV_CC_DEVICE_INFO deviceInfo = ReadDeviceInfo(deviceList.pDeviceInfo[index]);
                cameras.Add(ParseCameraInfo(deviceInfo));
            }
            return cameras;
        }

        public void Open()
        {
            lock (_syncRoot)
            {
                if (_isOpen)
                {
                    return;
                }

                MyCamera camera = new();
                MyCamera.MV_CC_DEVICE_INFO deviceInfo = FindDeviceInfo(_info);
                try
                {
                    int result = camera.MV_CC_CreateDevice_NET(ref deviceInfo);
                    EnsureSdkSuccess("创建海康相机", result);

                    result = camera.MV_CC_OpenDevice_NET();
                    EnsureSdkSuccess("打开海康相机", result);

                    if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        int packetSize = camera.MV_CC_GetOptimalPacketSize_NET();
                        if (packetSize > 0)
                        {
                            camera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)packetSize);

                        }
                    }

                    camera.MV_CC_SetEnumValue_NET("AcquisitionMode", 0);
                    result = camera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
                    EnsureSdkSuccess("设置相机触发模式", result);

                    _camera = camera;
                    _isOpen = true;
                }
                catch
                {
                    camera.MV_CC_CloseDevice_NET();
                    camera.MV_CC_DestroyDevice_NET();
                    throw;
                }
            }
        }

        public void Close()
        {
            lock (_syncRoot)
            {
                if (_isGrabbing)
                {
                    _camera?.MV_CC_StopGrabbing_NET();
                    _isGrabbing = false;
                }

                if (_camera != null)
                {
                    _camera.MV_CC_CloseDevice_NET();
                    _camera.MV_CC_DestroyDevice_NET();
                    _camera = null;
                }

                _isOpen = false;
            }
        }

        public BitmapSource GrabOne()
        {
            lock (_syncRoot)
            {
                EnsureOpened();

                if (!_isGrabbing)
                {
                    int result = _camera!.MV_CC_StartGrabbing_NET();
                    EnsureSdkSuccess("开始取流", result);
                    _isGrabbing = true;
                }

                var frame = new MyCamera.MV_FRAME_OUT();
                try
                {
                    int result = _camera!.MV_CC_GetImageBuffer_NET(ref frame, GrabTimeoutMs);
                    if (result != MyCamera.MV_OK)
                    {
                        throw new InvalidOperationException($"获取海康相机图像失败，错误码 0x{result:X8}");
                    }

                    return ConvertFrame(frame);
                }
                finally
                {
                    if (frame.pBufAddr != IntPtr.Zero)
                    {
                        _camera!.MV_CC_FreeImageBuffer_NET(ref frame);
                    }
                }
            }
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        private static MyCamera.MV_CC_DEVICE_INFO FindDeviceInfo(CameraInfo? target)
        {
            var deviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            int result = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref deviceList);
            EnsureSdkSuccess("枚举海康相机", result);

            if (deviceList.nDeviceNum == 0)
            {
                throw new InvalidOperationException("未找到海康相机");
            }

            if (target == null)
            {
                return ReadDeviceInfo(deviceList.pDeviceInfo[0]);
            }

            for (int index = 0; index < deviceList.nDeviceNum; index++)
            {
                MyCamera.MV_CC_DEVICE_INFO deviceInfo = ReadDeviceInfo(deviceList.pDeviceInfo[index]);
                CameraInfo cameraInfo = ParseCameraInfo(deviceInfo);
                if (string.Equals(cameraInfo.SerialNumber, target.SerialNumber, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(cameraInfo.Name, target.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return deviceInfo;
                }
            }

            throw new InvalidOperationException($"未找到匹配的海康相机：{target.Name}（序列号 {target.SerialNumber}）");
        }

        private static MyCamera.MV_CC_DEVICE_INFO ReadDeviceInfo(IntPtr pointer)
        {
            return Marshal.PtrToStructure<MyCamera.MV_CC_DEVICE_INFO>(pointer);
        }

        private static CameraInfo ParseCameraInfo(MyCamera.MV_CC_DEVICE_INFO deviceInfo)
        {
            string userDefinedName = string.Empty;
            string serialNumber = string.Empty;
            string modelName = string.Empty;
            string manufacturerName = string.Empty;

            if (deviceInfo.nTLayerType == MyCamera.MV_GIGE_DEVICE)
            {
                var gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)MyCamera.ByteToStruct(
                    deviceInfo.SpecialInfo.stGigEInfo,
                    typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                userDefinedName = gigeInfo.chUserDefinedName;
                serialNumber = gigeInfo.chSerialNumber;
                modelName = gigeInfo.chModelName;
                manufacturerName = gigeInfo.chManufacturerName;
            }
            else if (deviceInfo.nTLayerType == MyCamera.MV_USB_DEVICE)
            {
                var usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)MyCamera.ByteToStruct(
                    deviceInfo.SpecialInfo.stUsb3VInfo,
                    typeof(MyCamera.MV_USB3_DEVICE_INFO));
                userDefinedName = usbInfo.chUserDefinedName;
                serialNumber = usbInfo.chSerialNumber;
                modelName = usbInfo.chModelName;
                manufacturerName = usbInfo.chManufacturerName;
            }

            string name = string.IsNullOrWhiteSpace(userDefinedName)
                ? $"{manufacturerName} {modelName}".Trim()
                : userDefinedName;

            return new CameraInfo
            {
                Name = string.IsNullOrWhiteSpace(name) ? "HikCamera" : name,
                SerialNumber = serialNumber,
                Model = modelName,
                DriverName = "HikDriver"
            };
        }

        private void EnsureOpened()
        {
            if (!_isOpen)
            {
                Open();
            }

            if (_camera == null)
            {
                throw new InvalidOperationException("海康相机未打开");
            }
        }

        private static void EnsureSdkSuccess(string action, int result)
        {
            if (result != MyCamera.MV_OK)
            {
                throw new InvalidOperationException($"{action}失败，错误码 0x{result:X8}");
            }
        }

        private BitmapSource ConvertFrame(MyCamera.MV_FRAME_OUT frame)
        {
            MyCamera.MV_FRAME_OUT_INFO_EX frameInfo = frame.stFrameInfo;
            byte[] data;
            PixelFormat pixelFormat;
            int channels;

            if (frameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
            {
                data = CopyBuffer(frame.pBufAddr, frameInfo.nFrameLen);
                pixelFormat = PixelFormats.Gray8;
                channels = 1;
            }
            else if (frameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
            {
                data = CopyBuffer(frame.pBufAddr, frameInfo.nFrameLen);
                pixelFormat = PixelFormats.Rgb24;
                channels = 3;
            }
            else
            {
                bool isMono = IsMonoFormat(frameInfo.enPixelType);
                MyCamera.MvGvspPixelType destinationType = isMono
                    ? MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8
                    : MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                pixelFormat = isMono ? PixelFormats.Gray8 : PixelFormats.Rgb24;
                channels = isMono ? 1 : 3;
                data = ConvertPixelData(frame, destinationType, channels);
            }

            int stride = frameInfo.nWidth * channels;
            BitmapSource bitmap = BitmapSource.Create(
                frameInfo.nWidth,
                frameInfo.nHeight,
                96,
                96,
                pixelFormat,
                null,
                data,
                stride);
            bitmap.Freeze();
            return bitmap;
        }

        private byte[] ConvertPixelData(
            MyCamera.MV_FRAME_OUT frame,
            MyCamera.MvGvspPixelType destinationType,
            int channels)
        {
            int destinationSize = frame.stFrameInfo.nWidth * frame.stFrameInfo.nHeight * channels;
            IntPtr destination = Marshal.AllocHGlobal(destinationSize);
            try
            {
                var convertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM
                {
                    nWidth = frame.stFrameInfo.nWidth,
                    nHeight = frame.stFrameInfo.nHeight,
                    enSrcPixelType = frame.stFrameInfo.enPixelType,
                    pSrcData = frame.pBufAddr,
                    nSrcDataLen = frame.stFrameInfo.nFrameLen,
                    enDstPixelType = destinationType,
                    pDstBuffer = destination,
                    nDstBufferSize = (uint)destinationSize
                };

                int result = _camera!.MV_CC_ConvertPixelType_NET(ref convertParam);
                if (result != MyCamera.MV_OK)
                {
                    throw new InvalidOperationException($"转换海康相机图像像素格式失败，错误码 0x{result:X8}");
                }

                byte[] data = new byte[convertParam.nDstLen];
                Marshal.Copy(convertParam.pDstBuffer, data, 0, data.Length);
                return data;
            }
            finally
            {
                Marshal.FreeHGlobal(destination);
            }
        }

        private static byte[] CopyBuffer(IntPtr source, uint length)
        {
            byte[] data = new byte[length];
            Marshal.Copy(source, data, 0, data.Length);
            return data;
        }

        private static bool IsMonoFormat(MyCamera.MvGvspPixelType pixelType)
        {
            switch (pixelType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono1p:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono2p:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono4p:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8_Signed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono14:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono16:
                    return true;
                default:
                    return false;
            }
        }
    }
}
