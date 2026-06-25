using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using VisionView.ParamenterUILibary.ParameterModel;
using VisionView.ParameterUILibary.Core;

namespace VisionView.DataModel
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
        [ObservableProperty]
        private RunStatus _runStatus = RunStatus.Idle;//当前卡片有无执行完成
        [ObservableProperty]
        private int _runProgress = 0;//当前卡片的执行进度
        public ObservableCollection<CardDataModel> UpstreamCards { get; } = new();//存储上游卡片的集合
        /// <summary>
        /// 索引所有上游卡片的输出参数，返回一个列表，包含每个参数的完整路径（父卡片ID.参数Key）和参数类型
        /// </summary>
        /// <returns></returns>
        public List<InputOptionModel> GetAllUpstreamCard()
        {
            var CardMsg = new List<InputOptionModel>();
            foreach (var parent in UpstreamCards)
            {
                if (parent.ParameterVM is BaseParameter parentParam)
                {
                    foreach (var card in parentParam.OutParameter)
                    {
                        CardMsg.Add((new InputOptionModel
                        {
                            RealId = $"{parent.CardID}.{card.Key}",
                            DisplayName = $"{parent.CardName}.{card.Key}",
                            Type = card.Value
                        }));
                    }
                    // 2. 递归：让父节点去要它上面的节点输出，一起加进来
                    CardMsg.AddRange(parent.GetAllUpstreamCard());
                }
            }
            return CardMsg.DistinctBy(X => X.RealId).ToList();

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
                                .Select(d => d)
                                .ToList();
                //清空旧的下拉列表
                param.Options.Clear();
                foreach (var key in validKeys)
                {
                    //添加每个选项到列表
                    param.Options.Add(key);
                }
                //如果现在选择的选项不在当前的列表里，则选取第一个进行填充
                if (!param.Options.Any(o => o.RealId == param.SelectedValue))
                {
                    var firstOpt = param.Options.FirstOrDefault();
                    if (firstOpt == null)
                    {
                        param.SelectedValue = "";
                    }
                    else
                    {
                        param.SelectedValue = firstOpt.DisplayName;
                    }


                }
            }
        }
    }
}

