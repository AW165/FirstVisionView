using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionView.ParameterUILibary.Core
{
    /// <summary>
    /// 建立算子卡片，通过注册机制（Register 方法）把每种算子类型和对应的参数模型生产方法绑定在一起，UI 只需要调用 CreateOperator 方法并传入卡片类型，就能瞬间得到一个全新的参数模型实例，极大地简化了算子卡片的创建流程和维护成本。
    /// </summary>
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
