using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FirstVisionView.ParamenterUILibary.ParameterModel;
using FirstVisionView.ParameterUILibary.Core;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace FirstVisionView.DataModel
{
    public partial class CardDataModel : ObservableObject
    {

        public string CardID = Guid.NewGuid().ToString("N");
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
        [ObservableProperty]
        private bool _isRenaming = false;
        [ObservableProperty]
        private ObservableObject? _parameterVM = null;//算子类型
        [ObservableProperty]
        private string _cardType = "Default";
        [ObservableProperty]
        private bool _isEnable;
        public ObservableCollection<CardDataModel> UpstreamCards { get; } = new();
        public List<(string key, ParameterDataType Type)> GetAllUpstreamCard()
        {
            var results = new List<(string key, ParameterDataType Type)>();
            foreach (var parent in UpstreamCards)
            {
                if (parent.ParameterVM is BaseParameter parentParam)
                {
                    foreach (var card in parentParam.OutParameter)
                    {
                        results.Add(($"{parent.CardID}.{card.Key})", card.Value));
                    }
                    // 2. 递归：让父节点去要它上面的节点输出，一起加进来
                    results.AddRange(parent.GetAllUpstreamCard());
                }
            }
            return results.Distinct().ToList();
        }
        /// <summary>
        /// 刷新本卡片的所有下拉框（只装填类型匹配的密钥）
        /// </summary>
        public void RefreshInputOptions()
        {
            // 检查当前是否是空值或类型一致
            if (this.ParameterVM is not BaseParameter currentParam)
            {
                return; // 如果自己还没有被赋值 ParameterVM，或者类型不对，直接放弃刷新
            }
            //调用递归获取之前线上所有的数据结果
            var availableData = GetAllUpstreamCard();

            // 此时使用转换后的 currentParam 去获取 ParameterList下面的Options
            foreach (var param in currentParam.ParameterList.OfType<ComboboxParameterItem>())
            {
                var validKeys = availableData
                                .Where(d => d.Type == param.AcceptType)
                                .Select(d => d.key)
                                .ToList();
                //清空旧的下拉列表
                param.Options.Clear();
                foreach (var key in validKeys)
                {
                    //添加每个选项到列表
                    param.Options.Add(key);
                }
                //如果现在选择的选项不在当前的列表里，则选取第一个进行填充
                if (!param.Options.Contains(param.SelectedValue))
                {
                    param.SelectedValue = param.Options.FirstOrDefault() ?? "";
                }
            }
        }
    }
}
    
