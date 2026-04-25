using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FirstVisionView.ParameterUILibary.Core
{
    public static class OperatorFactory
    {
        // 核心工厂字典：Key 是 CardType(如 "Binaryzation")，Value 是一个生产委托(帮你 new 对象)
        private static readonly Dictionary<string, Func<ObservableObject>> _registry = new();

        // 注册方法：把机器识别码和生产说明书录入字典
        public static void Register(string cardType, Func<ObservableObject> creator)
        {
            if (!_registry.ContainsKey(cardType))
            {
                _registry.Add(cardType, creator);
            }
        }

        // 生产方法：UI 点击时调用，通过 CardType 瞬间创建出对应的参数模型
        public static ObservableObject? CreateOperator(string cardType)
        {
            if (_registry.TryGetValue(cardType, out var creator))
            {
                return creator(); // 执行委托，真正 new 出对象
            }
            return null; // 找不到对应算子时返回空
        }
    }
}
