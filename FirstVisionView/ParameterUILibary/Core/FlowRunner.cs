using VisionView.DataModel;
using VisionView.ParamenterUILibary.ParameterModel;

namespace VisionView.ParameterUILibary.Core
{
    public class FlowRunner
    {
        private List<Dictionary<string, RelyOnCard>> cardID = new();//存储每个卡片的依赖关系，字典的键是卡片ID，值是一个RelyOnCard对象，表示该卡片依赖的其他卡片ID列表和依赖数量

        public void RelyOnCollection(List<CardDataModel> allCards)
        {
            foreach (var card in allCards)//遍历所有卡片并为每个卡片创建一个RelyOnCard对象，存储该卡片依赖的其他卡片ID列表
            {
                List<string> relyOnCardIdList = new List<string>();//存储当前卡片依赖的其他卡片ID列表
                var CardList = GetRelyOnCardList(card);
                cardID.Add(new Dictionary<string, RelyOnCard>
                { { card.CardID, new RelyOnCard { RelyOnCardId = relyOnCardIdList } } });//将当前卡片的ID和RelyOnCard对象添加到cardID列表中

            }
        }
        public List<RelyOnCard> GetRelyOnCardList(CardDataModel card)
        {
            List<RelyOnCard> relyOnCardList = new List<RelyOnCard>();//存储当前卡片依赖的其他卡片ID列表
            if (card.ParameterVM != null)
            {
                var parameterVM = card.ParameterVM;//拿到当前卡片的参数视图模型
                if (parameterVM is BaseParameter parameter)
                {
                    foreach (var parame in parameter.ParameterList)
                    {
                        relyOnCardList.Add(new RelyOnCard { RelyOnCardId = parame.GetRealValue() });
                    }
                }


            }


            return relyOnCardList;
        }

    }

}
