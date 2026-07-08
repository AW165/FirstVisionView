namespace VisionView.ParameterUILibary.Core
{
    /// <summary>
    /// 数据类型枚举，定义了参数可能的数据类型，包括字符串、数字、图片、整数、区域、布尔值和位置等。这些类型可以用于参数输入输出的类型检查和界面展示。
    /// </summary>
    public enum ParameterDataType
    {
        String,
        Double,
        Image,
        Integer,
        Region,
        Boolean,
        Position
    }
    public enum RunStatus
    {
        Idle,       // 待机
        Running,    // 运行中
        Success,    // 成功
        Fail,       // 失败
        Error       // 错误
    }
    /// <summary>
    /// 一个输入选项模型类，用于表示参数的输入选项。它包含三个属性：RealId（真实ID）、DisplayName（显示名称）和Type（参数数据类型）。
    /// 该类还重写了ToString方法，以便在需要显示选项名称时返回DisplayName。
    /// </summary>
    public class InputOptionModel
    {
        public string RealId { get; set; }
        public string DisplayName { get; set; }
        public ParameterDataType Type { get; set; }
        public override string ToString()
        {
            return DisplayName;
        }
    }
    /// <summary>
    /// 每个卡片的依赖关系类，存储该卡片依赖的其他卡片ID列表和依赖数量
    /// </summary>
    public class RelyOnCard
    {
        public List<string> RelyOnCardId { get; set; } = new List<string>();
        public int RelyOnCardNum { get { return RelyOnCardId.Count; } set { } }
    }
}
