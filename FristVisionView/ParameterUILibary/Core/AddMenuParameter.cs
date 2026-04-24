using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FirstVisionView.ParameterUILibary.Core
{
    public partial class AddBaseMenuParameter : ObservableObject
    {
        [ObservableProperty]
        private string _name = "";
        [ObservableProperty]
        private string _bindCommand = "";
        [ObservableProperty]
        private string _commandMes = "";
        
    }
    //此处加标签
    public partial class AddButton : AddBaseMenuParameter
    { 
        
    }
}
