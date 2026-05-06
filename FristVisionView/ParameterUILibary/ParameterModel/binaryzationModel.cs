using System.Collections.ObjectModel;
using FirstVisionView.ParamenterUILibary.ParameterModel;
using FirstVisionView.ParameterUILibary.Core;
namespace FirstVisionView.ParameterUILibary.ParameterModel
{
    //二值化算子参数
    [VisionOperator("图像处理", "二值化", "BinaryzationModel")]
    public partial class BinaryzationModel : BaseParameter
    {
        public BinaryzationModel()
        {
            this.Title = "二值化";
            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "图像来源",
                AcceptType = ParameterDataType.Image,
                Options = new ObservableCollection<string> { },
            });
            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "ROI区域",
                AcceptType = ParameterDataType.Region,
                Options = new ObservableCollection<string> { },


            });
            //添加到输出字典中
            this.OutParameter["Image"] = ParameterDataType.Image;
            // 因为继承了 BaseParameter，所以可以直接调用 ParameterList
            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "阈值方式",
                Options = new ObservableCollection<string> { "双阈值", "高斯", "均值" },
                SelectedValue = "双阈值"
            });
            this.ParameterList.Add(new SliderParameterItem
            {
                Name = "阈值",
                Max = 255,
                Min = 0,
                Step = 1,

            });


            this.ParameterList.Add(new ComboboxParameterItem
            {
                Name = "位置修正参考",
                AcceptType = ParameterDataType.Position,
                Options = new ObservableCollection<string> { },
            });
        }
    }


}
