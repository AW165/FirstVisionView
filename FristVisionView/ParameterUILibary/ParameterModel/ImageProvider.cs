using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FirstVisionView.ParamenterUILibary.ParameterModel;
using FirstVisionView.ParameterUILibary.Core;

namespace FirstVisionView.ParameterUILibary.ParameterModel
{
    [VisionOperator("图像","图像源","ImageProvider")]
    public class ImageProvider : BaseParameter
    {
        public ImageProvider()
        {
            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "图像来源",
                Options = { "Local", "Camrea" },
                SelectedValue = "Local"
            });
          this.ParameterList.Add(new ComboboxParameterItem {
                Name = "像素格式",
                Options = { "MONO8", "RGB24" },
                SelectedValue = "MONO8"
            });
            this.ParameterList.Add(new ComboboxParameterItem() { 
                Name = "相机",
                Options = {},
                SelectedValue = ""
            });
            this.ParameterList.Add(new TextBoxParameterItem()
            {
                Name = "曝光",
                AccptType = ParameterDataType.Integer,
                PresentValue = 0
            });
            this.ParameterList.Add(new TextBoxParameterItem()
            {
                Name = "增益",
                AccptType = ParameterDataType.Double,
                PresentValue = 0
            });
        }
    }
}
