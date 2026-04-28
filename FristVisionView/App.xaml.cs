using System.Configuration;
using System.Data;
using System.Windows;
using FirstVisionView.Core;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;

namespace FirstVisionView;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // 🌟 这是绝对不能省略的“点火”步骤：扫描程序集、填充工厂、构建全局菜单树
        OperatorRegistry.Initialize();
}
}

