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

namespace FirstVisionView.ViewModels
{
  public partial class AdjustViewModel :ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<CardDataModel> _allCards = new();

        [RelayCommand]
        private void AddCard(string? cardName = null)
        {
            CardDataModel newCardData = new CardDataModel()
            {
                X = 100, // 给个初始测试坐标 X
                Y = 100, // 给个初始测试坐标 Y
                CardName = "MVVM 新卡片"
            };
            AllCards.Add(newCardData);
      
        }
        [RelayCommand]
        private void DelateCards()
        {
            //把符合选中条件的选出并生成一个列表，防止直接操作原列表导致崩溃
            var deletecard = AllCards.Where(s => s.IsSelected).ToList();
            if (deletecard.Count == 0) return;
            foreach (var card in deletecard)
            {
                    AllCards.Remove(card);
                
            }

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
            return;
        }
        [RelayCommand]
        private void AddSeletionStatus(CardDataModel? card = null)

        {
            if (card == null) return;//如果未指定卡片，返回，否则设置传入卡片的状态
            card.IsSelected = true;
        }

       

        

    }
}
