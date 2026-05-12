using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;

namespace VisionView.DataModel
{
    /// <summary>
    /// 生产记录
    /// 这个类 = 数据库里的一张表
    /// 类的每个属性 = 表里的一列
    /// </summary>
    [Table (Name ="Production_Log")]
    internal class ProductionLog
    {
        [Column(IsIdentity =true,IsPrimary = true)]
        //数据库表单
        //数据ID
        public int Id { get; set; }
        //创建时间
        public DateTime Timer { get; set; }
        //产品名称
        public String ProductName {  get; set; }
        //测试状态
        public string Status { get; set; }
        //产品ID
        public String ProductId {  get; set; }

       

    }
}
