using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FirstVisionView.Card;
using FirstVisionView.DataModel;
using OpenTK.Graphics.ES11;

namespace FirstVisionView.ViewModels
{
  public partial class AdjustViewModel :ObservableObject
    {
        [ObservableProperty] private ObservableCollection<WireDataModel> _allWires = new();
        [ObservableProperty] private ObservableCollection<CardDataModel> _allCards = new();
        [ObservableProperty] private bool _delePopup = false;
        [ObservableProperty] private bool _addPopup = false;
        public bool CanRename => (AllCards.Count(c => c.IsSelected) == 1);
        // 用来记录刚才右键点击的位置
        public System.Drawing.PointF CurrentMousePoint { get; set; }
        private int _topZIndex = 0;
        [RelayCommand]
        private void AddCard()
    {
        // 1. 获取鼠标点击的初始期待坐标
        double spawnX = CurrentMousePoint.X;
        double spawnY = CurrentMousePoint.Y;
            

        // 🌟 2. 智能防重叠算法 (while 循环检测)
        // 逻辑：去所有的卡片里找，有没有哪张卡片的左上角坐标，跟我要生成的坐标“靠得太近”（比如正负 10 像素以内）
        // 如果有，说明位置被占了，我就向右下方挪动 30 像素，然后再查一次，直到找到空位！
        while (AllCards.Any(c => Math.Abs(c.X - spawnX) < 10 && Math.Abs(c.Y - spawnY) < 10))
        {
            spawnX += 30; // X 向右偏
            spawnY += 30; // Y 向下偏
        }

        // 3. 找到空位了！新建卡片并赋坐标
        CardDataModel newCard = new CardDataModel()
        {
            CardName = "新参数卡",
            X = spawnX,
            Y = spawnY,
            // ... 其他属性
        };

        AllCards.Add(newCard);
        
        // 添加完记得关闭菜单
        AddPopup = false; 
    }
        [RelayCommand]
        private void DeleteCards()
        {
            //把符合选中条件的选出并生成一个列表，防止直接操作原列表导致崩溃
            var deletecard = AllCards.Where(s => s.IsSelected).ToList();
            if (deletecard.Count == 0) return;
            foreach (var card in deletecard)
            {
                    AllCards.Remove(card);
                
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



    }
}
