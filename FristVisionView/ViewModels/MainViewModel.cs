using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Text;

namespace FirstVisionView.ViewModels
{
    public partial class MainViewModels : ObservableObject

    {
        
        private readonly AdjustPage _AdjustPage = new();
        private readonly DataPage _DataPage = new();
        private readonly SettingPage _SettingPage = new();
        [ObservableProperty] private bool _FilePopup = false;
        [ObservableProperty] private bool _ToolPopup = false;       
        [ObservableProperty] private bool _ViewPopup = false;
        [ObservableProperty] private UserControl _CurrentPage;
        [ObservableProperty] private AdjustViewModel _adjustVM = new();



        public MainViewModels()
        {
            _CurrentPage = _AdjustPage;
        }
        [RelayCommand]
        private void SwitchPage(string? propertyName = null)
        {
            switch (propertyName)
            {
                case "Adjust":
                    CurrentPage = _AdjustPage;
                break;
                case "Data":
                    CurrentPage = _DataPage;
                    break;
                case "Setting":
                    CurrentPage = _SettingPage;
                break;

            }
        }
        [RelayCommand]
        private void SwitchOpenPopup(string? propertyName = null)
        {
           FilePopup = false;
           ToolPopup = false;
           ViewPopup = false;
            switch (propertyName)
            {
                case "File": FilePopup = true; break;
                case "Tool": ToolPopup = true; break;
                case "View": ViewPopup = true; break;
            }
        }
    }
}