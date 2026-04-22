using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstVisionView.Card;
using FirstVisionView.DataModel;
using FirstVisionView.ParameterUILibary.Core;
using FirstVisionView.ParameterUILibary.ParameterModel;
using OpenTK.Graphics.ES11;

namespace FirstVisionView.ViewModels
{
  public partial class AdjustViewModel :ObservableObject
    {
        [ObservableProperty] private ObservableCollection<WireDataModel> _allWires = new();
        [ObservableProperty] private ObservableCollection<CardDataModel> _allCards = new();
        [ObservableProperty] private bool _delePopup = false;
        [ObservableProperty] private bool _addPopup = false;
        [ObservableProperty] private bool _cap = false;
        [ObservableProperty] private ObservableObject? _currentEditVM = null;
        public bool CanRename => (AllCards.Count(c => c.IsSelected) == 1);
        // 用来记录刚才右键点击的位置
        public System.Drawing.PointF CurrentMousePoint { get; set; }
        private int _topZIndex = 0;
        public AdjustViewModel()
        {
            AllCards.CollectionChanged += AllWires_CollectionChanged;
        }
        private void AllWires_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (WireDataModel wire in e.NewItems)
                {
                    if (!wire.TargetCard.UpstreamCards.Contains(wire.SourceCard))
                    {
                        //把连接的卡片存储在当前卡片的来源里
                        wire.TargetCard.UpstreamCards.Add(wire.SourceCard);
                    }
                    //刷新
                    wire.TargetCard.RefreshInputOptions();
                }
            }
            if (e.OldItems != null)
            {
                foreach (WireDataModel wire in e.OldItems)
                {
                    // 断开连线时，关系并重新刷新
                    wire.TargetCard.UpstreamCards.Remove(wire.SourceCard);
                    wire.TargetCard.RefreshInputOptions();
                }
            }
        }
        /// <summary>
        /// 新建卡片，设置卡片的名字，
        /// </summary>
        /// <param name="CardType"></param>
        [RelayCommand]
        private void AddCard(string CardType)
        {
            // 1. 获取鼠标点击的初始期待坐标
            double spawnX = CurrentMousePoint.X;
            double spawnY = CurrentMousePoint.Y;
       
        // 找到空位
        while (AllCards.Any(c => Math.Abs(c.X - spawnX) < 10 && Math.Abs(c.Y - spawnY) < 10))
        {
            spawnX += 30; // X 向右偏
            spawnY += 30; // Y 向下偏
        }

        
            // 3. 找到空位新建卡片并赋坐标
            CardDataModel newCard = new CardDataModel()
            {
                CardName = ParameterCardName(CardType) ,
                X = spawnX,
                Y = spawnY,
                IsEnable = true,
                ParameterVM = ParameterCardModel(CardType)//
                // ... 其他属性
            };

        AllCards.Add(newCard);
        
        // 关闭菜单
        AddPopup = false; 
    }
        /// <summary>
        /// 算子生成方法，传入字符串，返回new出的算子实例
        /// </summary>
        /// <param name="CardType"></param>
        /// <returns></returns>
        private ObservableObject? ParameterCardModel(string CardType)
        {
            return CardType switch
            {
                "Binaryzation" => new BinaryzationModel(),//创建一个二值化算子
                _ => null,
            };   
        }
        private string ParameterCardName(string CardType)
        {
            var index = (AllCards.Count()+1).ToString();
            return CardType switch
            {
                "Binaryzation" => $"二值化{index}",
                _ => $"算法{index}",
            };   
        }
        [RelayCommand]
        private void DeleteCards()
        {
            //把符合选中条件的选出并生成一个列表，防止直接操作原列表导致崩溃
            var deleteCard = AllCards.Where(s => s.IsSelected).ToList();
            var deleteWire = AllWires.Where(s => s.IsSelected).ToList();
            if (deleteCard.Count != 0 )
            {
                foreach (var card in deleteCard)//删除卡片
                {
                    AllCards.Remove(card);
                    var WireList = AllWires.ToList();
                    foreach (var wire in WireList)
                    {
                        if (wire.SourceCard.CardID == card.CardID || wire.TargetCard.CardID == card.CardID)
                        {
                            AllWires.Remove(wire);
                        }
                    }

                }
            }
            
            if (deleteWire.Count != 0)
            {
                foreach (var wire in deleteWire)//删除线
                {
                    AllWires.Remove(wire);

                }
                
            }
            DelePopup = false;//关闭删除菜单
        }
        
        [RelayCommand]
        private void ClearSeletionStatus(CardDataModel? card = null)

        {
            if (card == null)//如果未指定卡片，则清除所有选择状态的卡片，否则清除传入卡片的状态
            {
                    foreach (var SetCard in AllCards.Where(c=> c.IsSelected))
                    {
                       SetCard.IsSelected = false;
                    } 
            }
            else
            {
                card.IsSelected = false;
            }
            OnPropertyChanged(nameof(CanRename));

            return;
        }
        [RelayCommand]
        private void AddSeletionStatus(CardDataModel? card = null)

        {
            if (card == null) return;//如果未指定卡片，返回，否则设置传入卡片的状态
            _topZIndex++;
            card.TopZIndex = _topZIndex;
            card.IsSelected = true;
            if (_topZIndex >= 1000000)
            {
                _topZIndex = 0;
                var list = AllCards.OrderBy(c => c.TopZIndex).ToList();
                foreach (var cardZIndex in list)
                {
                   
                    _topZIndex++;
                    cardZIndex.TopZIndex = _topZIndex;
                    
                }
            }
           
            OnPropertyChanged(nameof(CanRename));

        }
        [RelayCommand]
        private void CardRename()
        {
            var card = AllCards.FirstOrDefault(c => c.IsSelected);
            if (card == null) return;
            card.IsRenaming = true;
            DelePopup = false;
        }

        [RelayCommand]
        private void WireAddStatus(WireDataModel? wire = null)
        {
            if (wire == null) return;
            wire.IsSelected = true;
        }
        [RelayCommand]
        private void WireCleanStatus(WireDataModel? wire = null)
        {
            if (wire == null)
            {
                var WireList = AllWires.Where(s => s.IsSelected).ToList();
                foreach (var Cleanwire in WireList)
                {
                    Cleanwire.IsSelected = false;
                }
            }
            else wire.IsSelected = false;
        }
        [RelayCommand]
        private void ParameterClick()
        {
            Cap = false;
        }
       [RelayCommand]
        private void ChangeStatus(CardDataModel card)
        {
            if (card == null) return;
            card.IsEnable = !card.IsEnable;//反转启用状态
        }
    }
}
