using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using VisionView.HardWare;
using VisionView.ParamenterUILibary.ParameterModel;
using VisionView.ParameterUILibary.Core;

namespace VisionView.ParameterUILibary.ParameterModel
{
    [VisionOperator("图像", "图像源", "ImageProvider")]
    public class ImageProvider : BaseParameter
    {
        public ImageProvider()
        {
            List<CameraInfo> cameras = LoadCameras();

            var ImageSoure = (new ComboboxParameterItem
            {
                Name = "图像来源",
                Options =
                {
                    new InputOptionModel { RealId = "Local", DisplayName = "本地图片" },
                    new InputOptionModel { RealId = "Camera", DisplayName = "相机" }
                },
                SelectedValue = "Local"
            });

            var Camera = (new ComboboxParameterItem()
            {
                Name = "相机",
                Options = { },
                SelectedValue = "",
                IsVisible = false
            });
            foreach (var camera in cameras)
            {
                string display = string.IsNullOrWhiteSpace(camera.Name) ? camera.Model : camera.Name;
                if (!string.IsNullOrWhiteSpace(camera.Model)
                    && !display.Contains(camera.Model, StringComparison.OrdinalIgnoreCase))
                {
                    display += $" ({camera.Model})";
                }
                if (!string.IsNullOrWhiteSpace(camera.SerialNumber))
                {
                    display += $" [{camera.SerialNumber}]";
                }
                Camera.Options.Add(new InputOptionModel
                {
                    RealId = camera.SerialNumber,
                    DisplayName = display
                });
            }
            var Exposure = (new TextBoxParameterItem()
            {
                Name = "曝光",
                AcceptType = ParameterDataType.Integer,
                PresentValue = 0,
                IsVisible = false
            });
            var Gian = (new TextBoxParameterItem()
            {
                Name = "增益",
                AcceptType = ParameterDataType.Integer,
                PresentValue = 0,
                IsVisible = false
            });
            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "像素格式",
                Options = { new InputOptionModel { DisplayName = "MONO8" }, new InputOptionModel { DisplayName = "RGB24" } },
                SelectedValue = "MONO8"
            });

            ImageSoure.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ComboboxParameterItem.SelectedValue))
                {
                    if (ImageSoure.SelectedValue == "Camera")
                    {
                        Camera.IsVisible = true;
                        Exposure.IsVisible = true;
                        Gian.IsVisible = true;
                        if (string.IsNullOrEmpty(Camera.SelectedValue) && Camera.Options.Count > 0)
                        {
                            Camera.SelectedValue = Camera.Options[0].RealId;
                        }
                    }
                    else
                    {
                        Camera.IsVisible = false;
                        Exposure.IsVisible = false;
                        Gian.IsVisible = false;
                    }
                }
            };

            Camera.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(ComboboxParameterItem.SelectedValue)) return;
                if (ImageSoure.SelectedValue != "Camera" || string.IsNullOrEmpty(Camera.SelectedValue)) return;

                var camera = cameras.FirstOrDefault(c => c.SerialNumber == Camera.SelectedValue);
                if (camera != null)
                {
                    WeakReferenceMessenger.Default.Send(new CameraGrabMessage(camera));
                }
            };

            this.ParameterList.Add(ImageSoure);
            this.ParameterList.Add(Camera);
            this.ParameterList.Add(Exposure);
            this.ParameterList.Add(Gian);
        }

        private static List<CameraInfo> LoadCameras()
        {
            try
            {
                var manager = new CameraManager();
                manager.LoadDrivers();
                return manager.GetAllCameras();
            }
            catch
            {
                return new List<CameraInfo>();
            }
        }
    }
}
