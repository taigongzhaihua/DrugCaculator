using System.Collections.ObjectModel;

namespace DrugCaculator.Models
{
    public class Drug
    {
        public int Id { get; set; }
        public string Name { get; set; } // 药物名称
        public ObservableCollection<DrugCalculationRule> CalculationRules { get; set; } = []; // 计算规则集合
        public string Description { get; set; } // 药物描述
        public string Usage { get; set; } // 用法
        public string Specification { get; set; } // 规格
    }
}