using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirstVisionView.ParamenterUILibary.ParameterModel;
using FirstVisionView.ParameterUILibary.Core;

namespace FirstVisionView.ParameterUILibary.ParameterModel
{
    public partial class AddParameterCard : AddParameter
    {
        public AddParameterCard()
        {
            this.AddParameterList.Add(new AddButton
            {
                Name = "图像源",
                BindCommand = "",
                CommandMes = "图像源"

            });
            this.AddParameterList.Add(new AddButton
            {
                Name = "图像源",
                BindCommand = "",

            });

            this.AddParameterList.Add(new AddButton
            {
                Name = "二值化",
                BindCommand = "",

            });

            this.AddParameterList.Add(new AddButton
            {
                Name = "逻辑运算",
                BindCommand = "",

            });
            this.AddParameterList.Add(new AddButton
            {
                Name = "通信",
                BindCommand = "",

            });
        }
    }
}
