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

    }
}
