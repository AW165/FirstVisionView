using VisionView.DataModel;

namespace VisionView.ParameterUILibary.Core
{
    public static class DAG
    {
        /// <summary>
        /// 传入源卡片，目标卡片，所有已连接线段，返回检查结果，ture代表回环，false代表无回环
        /// </summary>
        /// <param name="SourceCard"></param>
        /// <param name="TargetCard"></param>
        /// <param name="allWires"></param>
        /// <returns></returns>
        public static bool IsCycle(CardDataModel SourceCard, CardDataModel TargetCard, IEnumerable<WireDataModel> allWires)
        {
            if (SourceCard.CardID == TargetCard.CardID)
            {
                return true;
            }
            HashSet<string> visited = new HashSet<string>();//记录访问过的节点
            Queue<CardDataModel> queue = new Queue<CardDataModel>();//使用队列进行广度优先搜索
            queue.Enqueue(TargetCard);//从目标节点开始搜索
            while (queue.Count > 0)//只要队列不空，就继续搜索
            {
                var current = queue.Dequeue();//取出一个最前的节点
                if (current.CardID == SourceCard.CardID) return true;//如果找到了源节点，说明存在路径
                var children = GetDirectChildren(current, allWires);//获取当前节点的直接子节点
                foreach (var child in children)
                {
                    if (!visited.Contains(child.CardID))//如果不在黑名单里，说明这个子节点还没有被访问过
                    {
                        visited.Add(child.CardID);//标记这个子节点已经访问过了
                        if (child.CardID == SourceCard.CardID) return true;//如果直接子节点就是目标节点，说明存在路径      
                        queue.Enqueue(child);//将这个子节点加入队列，继续搜索
                    }

                }

            }
            return false;
        }
        private static IEnumerable<CardDataModel> GetDirectChildren(CardDataModel parent, IEnumerable<WireDataModel> allWires)
        {
            return allWires
                .Where(wire => wire.SourceCard != null && wire.SourceCard.CardID == parent.CardID)
                .Select(wire => wire.TargetCard);
        }
    }
}
