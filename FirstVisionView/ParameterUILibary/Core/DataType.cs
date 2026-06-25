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
    public class RelyOnCard
    {
        public List<string> RelyOnCardId { get; set; } = new List<string>();
        public int RelyOnCardNum { get { return RelyOnCardId.Count; } set { } }
    }
}
