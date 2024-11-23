using DrugCalculator.DataAccess;
using DrugCalculator.Models;
using NLog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace DrugCalculator.Services
{
    /// <summary>
    /// 药物服务类。
    /// 提供与药物和药物计算规则相关的数据库操作，包括添加、更新、删除和查询功能。
    /// </summary>
    public class DrugService
    {
        private readonly DbManager _dbManager;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 构造函数，初始化数据库连接并确保数据库结构完整。
        /// </summary>
        public DrugService()
        {
            const string connectionString = "Data Source=|DataDirectory|\\drugs.db;Version=3;";
            _dbManager = new DbManager(connectionString);

            // 确保数据库表结构存在
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化数据库结构，创建表结构。
        /// 如果表 Drug 和 DrugCalculationRule 不存在，则会创建它们。
        /// </summary>
        private void InitializeDatabase()
        {
            // 创建存储药物信息的表 Drug
            _dbManager.CreateTableIfNotExists("Drug",
                ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"), // 药物唯一标识
                ("Name", "TEXT NOT NULL"),                  // 药物名称
                ("Description", "TEXT"),                   // 药物描述
                ("Usage", "TEXT"),                         // 使用方法
                ("Specification", "TEXT")                 // 药物规格
            );

            // 创建存储药物计算规则的表 DrugCalculationRule
            _dbManager.CreateTableIfNotExists("DrugCalculationRule",
                ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"), // 规则唯一标识
                ("DrugId", "INTEGER"),                      // 关联药物的唯一标识
                ("Condition", "TEXT"),                      // 适用条件
                ("Formula", "TEXT"),                        // 计算公式
                ("Unit", "TEXT"),                           // 计量单位
                ("Frequency", "TEXT"),                      // 给药频率
                ("Route", "TEXT"),                          // 给药途径
                ("FOREIGN KEY (DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE") // 外键关联
            );
        }

        /// <summary>
        /// 获取所有药物及其关联的计算规则。
        /// </summary>
        /// <returns>包含所有药物及其规则的集合。</returns>
        public IEnumerable<Drug> GetAllDrugs()
        {
            // 查询所有药物信息
            var drugs = _dbManager.Query<Drug>("Drug");
            var drugList = drugs as Drug[] ?? drugs.ToArray();

            // 为每种药物加载其关联的计算规则
            foreach (var drug in drugList)
            {
                // 查询当前药物的计算规则
                var rules = _dbManager.Query<DrugCalculationRule>(
                    "DrugCalculationRule",
                    "WHERE DrugId = @DrugId",
                    "DrugId", drug.Id);

                // 将规则添加到药物对象的 CalculationRules 集合中
                drug.CalculationRules = new ObservableCollection<DrugCalculationRule>(rules);
            }

            return drugList;
        }

        /// <summary>
        /// 添加单个药物及其计算规则。
        /// </summary>
        /// <param name="drug">包含药物信息和规则的 Drug 对象。</param>
        public void AddDrug(Drug drug)
        {
            // 插入药物信息，返回生成的药物 ID
            var drugId = _dbManager.Insert(
                "Drug",
                ("Name", drug.Name),
                ("Description", drug.Description),
                ("Usage", drug.Usage),
                ("Specification", drug.Specification)
            );

            // 插入药物关联的计算规则
            foreach (var rule in drug.CalculationRules)
            {
                rule.DrugId = drugId;
                AddCalculationRule(rule);
            }
        }

        /// <summary>
        /// 从 DataTable 中批量添加药物。
        /// 每一行代表一个药物记录，DataTable 的列名称应匹配药物字段。
        /// </summary>
        /// <param name="dataTable">包含药物数据的 DataTable。</param>
        public void AddDrugsFromTable(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                Logger.Warn("DataTable 为空，未插入任何记录。");
                return;
            }

            foreach (DataRow row in dataTable.Rows)
            {
                // 将 DataRow 转换为 Drug 对象
                var drug = new Drug
                {
                    Name = row["Name"].ToString(),
                    Description = row["Description"].ToString(),
                    Usage = row["Usage"].ToString(),
                    Specification = row["Specification"].ToString(),
                    CalculationRules = [] // 规则列表为空，规则需单独添加
                };

                // 添加药物记录
                AddDrug(drug);
            }
        }

        /// <summary>
        /// 更新药物信息及其关联的计算规则。
        /// 如果药物的规则已存在，则更新；否则插入新的规则。
        /// </summary>
        /// <param name="drug">包含更新信息的药物对象。</param>
        public void UpdateDrug(Drug drug)
        {
            // 更新药物基本信息
            _dbManager.Update(
                "Drug",
                [
                    ("Name", drug.Name),
                    ("Description", drug.Description),
                    ("Usage", drug.Usage),
                    ("Specification", drug.Specification)
                ],
                "WHERE Id = @Id",
                "Id", drug.Id
            );

            // 获取数据库中现存的规则
            var existingRules = _dbManager.Query<DrugCalculationRule>(
                "DrugCalculationRule",
                "WHERE DrugId = @DrugId",
                "DrugId", drug.Id).ToList();

            // 当前药物的所有规则 ID
            var updatedRuleIds = drug.CalculationRules.Select(r => r.Id).ToHashSet();

            // 删除数据库中多余的规则
            foreach (var existingRule in existingRules.Where(existingRule => !updatedRuleIds.Contains(existingRule.Id)))
            {
                DeleteCalculationRule(existingRule.Id);
            }

            // 更新或新增规则
            foreach (var rule in drug.CalculationRules)
            {
                if (rule.Id == 0)
                {
                    // 插入新规则
                    rule.DrugId = drug.Id;
                    AddCalculationRule(rule);
                }
                else
                {
                    // 更新现有规则
                    UpdateCalculationRule(rule);
                }
            }
        }

        /// <summary>
        /// 删除指定药物及其关联的规则。
        /// </summary>
        /// <param name="id">药物 ID。</param>
        public void DeleteDrug(int id)
        {
            _dbManager.Delete("Drug", "WHERE Id = @Id", "Id", id);
        }

        /// <summary>
        /// 删除指定计算规则。
        /// </summary>
        /// <param name="id">规则 ID。</param>
        public void DeleteCalculationRule(int id)
        {
            _dbManager.Delete("DrugCalculationRule", "WHERE Id = @Id", "Id", id);
        }

        /// <summary>
        /// 添加计算规则。
        /// </summary>
        /// <param name="rule">包含规则信息的 DrugCalculationRule 对象。</param>
        private void AddCalculationRule(DrugCalculationRule rule)
        {
            _dbManager.Insert(
                "DrugCalculationRule",
                ("DrugId", rule.DrugId),
                ("Condition", rule.Condition),
                ("Formula", rule.Formula),
                ("Unit", rule.Unit),
                ("Frequency", rule.Frequency),
                ("Route", rule.Route)
            );
        }

        /// <summary>
        /// 更新计算规则。
        /// </summary>
        /// <param name="rule">包含更新信息的 DrugCalculationRule 对象。</param>
        private void UpdateCalculationRule(DrugCalculationRule rule)
        {
            _dbManager.Update(
                "DrugCalculationRule",
                [
                    ("Condition", rule.Condition),
                    ("Formula", rule.Formula),
                    ("Unit", rule.Unit),
                    ("Frequency", rule.Frequency),
                    ("Route", rule.Route)
                ],
                "WHERE Id = @Id",
                "Id", rule.Id
            );
        }
    }
}
