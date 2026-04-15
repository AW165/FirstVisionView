
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FirstVisionView.ParamenterUILibary.ParameterModel;
using FirstVisionView.ParameterUILibary.Core;
namespace FirstVisionView.ParameterUILibary.ParameterModel
{
    //二值化算子参数
    public partial class BinaryzationModel : BaseParamenter
    {
        public BinaryzationModel()
        {
            // 因为继承了 BaseParamenter，所以可以直接调用 ParameterList
            this.ParameterList.Add(new SliderParameterItem { 
                Name = "阈值" ,
                Max = 255,
                Min = 0,
                Step = 1,
            });
            this.Title = "二值化";
        }
    }
   
    
}
