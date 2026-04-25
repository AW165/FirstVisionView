using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstVisionView.ParameterUILibary.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    
    public class VisionOperatorAttribute : Attribute
    {
        public string Category { get; }//定义类别
        public string DispalyName { get; }//定义显示的名称
        public string ParameterType {  get; }//算子实例名称
        public VisionOperatorAttribute(string category, string dispalyName, string parameterType)//利用构造函数强制要求必须传入三个参数
        {
            Category = category;
            DispalyName = dispalyName;
            ParameterType = parameterType;
        }
    }
}
