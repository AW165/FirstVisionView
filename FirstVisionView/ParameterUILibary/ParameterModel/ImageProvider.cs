using VisionView.ParamenterUILibary.ParameterModel;
using VisionView.ParameterUILibary.Core;

namespace VisionView.ParameterUILibary.ParameterModel
{
    [VisionOperator("图像", "图像源", "ImageProvider")]
    public class ImageProvider : BaseParameter
    {
        public ImageProvider()
        {

            var ImageSoure = (new ComboboxParameterItem
            {
                Name = "图像来源",
                Options = { new InputOptionModel { DisplayName = "Local" }, new InputOptionModel { DisplayName = "Camrea" } },
                SelectedValue = "Local"
            });

            var Camera = (new ComboboxParameterItem()
            {
                Name = "相机",
                Options = { },
                SelectedValue = "",
                IsVisible = false

            });
            var Exposure = (new TextBoxParameterItem()
            {
                Name = "曝光",
                AccptType = ParameterDataType.Integer,
                PresentValue = 0,
                IsVisible = false
            });
            var Gian = (new TextBoxParameterItem()
            {
                Name = "增益",
                AccptType = ParameterDataType.Double,
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
                    if (ImageSoure.SelectedValue == "Camrea")
                    {
                        Camera.IsVisible = true;
                        Exposure.IsVisible = true;
                        Gian.IsVisible = true;
                    }
                    else
                    {
                        Camera.IsVisible = false;
                        Exposure.IsVisible = false;
                        Gian.IsVisible = false;
                    }
                }
            };

            this.ParameterList.Add(ImageSoure);
            this.ParameterList.Add(Camera);
            this.ParameterList.Add(Exposure);
            this.ParameterList.Add(Gian);

        }
    }
}
