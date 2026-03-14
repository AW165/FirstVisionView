using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FirstVisionView.DataModel
{
    public partial class CardDataModel : ObservableObject
    {
        [ObservableProperty]
        private double _x = 0;
        [ObservableProperty]
        private double _y = 0;
        [ObservableProperty]
        private bool _isSelected;
        [ObservableProperty]
        private string _cardName = "参数";
        [ObservableProperty]
        private int _topZIndex = 0;
    }
}
