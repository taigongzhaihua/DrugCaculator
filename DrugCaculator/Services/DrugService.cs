using DrugCalculator.DataAccess;
using DrugCalculator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace DrugCalculator.Services;

public class DrugService
{
    private readonly DbManager _dbManager;

    public DrugService()
    {
        const string connectionString = "Data Source=|DataDirectory|\\drugs.db;Version=3;";
        _dbManager = new DbManager(connectionString);
        InitializeDatabase();
    }

    // 初始化数据库结构
    private void InitializeDatabase()
    {
        _dbManager.CreateTableIfNotExists("Drug",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("Name", "TEXT NOT NULL"),
            ("Description", "TEXT"),
            ("Usage", "TEXT"),
            ("Specification", "TEXT")
        );

        _dbManager.CreateTableIfNotExists("DrugCalculationRule",
            ("Id", "INTEGER PRIMARY KEY AUTOINCREMENT"),
            ("DrugId", "INTEGER"),
            ("Condition", "TEXT"),
            ("Formula", "TEXT"),
            ("Unit", "TEXT"),
            ("Frequency", "TEXT"),
            ("Route", "TEXT"),
            ("FOREIGN KEY (DrugId)", "REFERENCES Drug(Id) ON DELETE CASCADE")
        );
    }

    // 获取所有药物及其规则
    public IEnumerable<Drug> GetAllDrugs()
    {
        var drugs = _dbManager.Query<Drug>("Drug");
        var enumerable = drugs as Drug[] ?? drugs.ToArray();
        foreach (var drug in enumerable)
        {
            var rules = _dbManager.Query<DrugCalculationRule>("DrugCalculationRule", "WHERE DrugId = @DrugId", "DrugId",
                drug.Id);
            drug.CalculationRules = new ObservableCollection<DrugCalculationRule>(rules);
        }

        return enumerable;
    }

    // 添加药物及其规则
    public void AddDrug(Drug drug)
    {
        var drugId = _dbManager.Insert(
            "Drug",
            ("Name", drug.Name),
            ("Description", drug.Description),
            ("Usage", drug.Usage),
            ("Specification", drug.Specification)
        );

        foreach (var rule in drug.CalculationRules)
        {
            rule.DrugId = drugId;
            AddCalculationRule(rule);
        }
    }

    public void AddDrugsFromTable(DataTable dataTable)
    {
        if (dataTable == null || dataTable.Rows.Count == 0)
        {
            Console.WriteLine(@"DataTable is empty. No records to insert.");
            return;
        }

        foreach (DataRow row in dataTable.Rows)
        {
            var drug = new Drug
            {
                Name = row["Name"].ToString(),
                Description = row["Description"].ToString(),
                Usage = row["Usage"].ToString(),
                Specification = row["Specification"].ToString(),
                CalculationRules = []
            };
            AddDrug(drug);
        }
    }

    // 更新药物及其规则
    public void UpdateDrug(Drug drug)
    {
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

        // 更新现有的计算规则或插入新的规则
        var existingRules = _dbManager.Query<DrugCalculationRule>("DrugCalculationRule", "WHERE DrugId = @DrugId", "DrugId", drug.Id).ToList();
        var updatedRuleIds = drug.CalculationRules.Select(r => r.Id).ToHashSet();

        // 删除多余的规则
        foreach (var existingRule in existingRules)
        {
            if (!updatedRuleIds.Contains(existingRule.Id))
            {
                DeleteCalculationRule(existingRule.Id);
            }
        }

        foreach (var rule in drug.CalculationRules)
        {
            if (rule.Id == 0)
            {
                // 新的规则，插入到数据库中
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

    // 更新计算规则
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

    // 删除药物及其规则
    public void DeleteDrug(int id)
    {
        _dbManager.Delete("Drug", "WHERE Id = @Id", "Id", id);
    }

    // 添加计算规则
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

    // 删除计算规则
    public void DeleteCalculationRule(int id)
    {
        _dbManager.Delete("DrugCalculationRule", "WHERE Id = @Id", "Id", id);
    }
}
