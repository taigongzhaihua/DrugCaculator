using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace DrugCaculator.Models
{
    public class DbManager
    {
        // 数据库连接字符串
        private const string ConnectionString = "Data Source=|DataDirectory|\\drugs.db;Version=3;";

        // 构造函数：在初始化时检查并创建数据库
        public DbManager()
        {
            // 检查并创建数据库
            InitializeDatabase();
        }

        // 初始化数据库
        private void InitializeDatabase()
        {
            var dbFilePath = GetDatabaseFilePath();
            if (File.Exists(dbFilePath)) return;
            SQLiteConnection.CreateFile(dbFilePath);
            CreateTables();
        }

        // 获取数据库文件的完整路径
        private static string GetDatabaseFilePath()
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory")?.ToString();
            return Path.Combine(dataDirectory ?? AppDomain.CurrentDomain.BaseDirectory, "drugs.db");
        }

        // 创建表结构
        private void CreateTables()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // 启用外键支持
            connection.Execute("PRAGMA foreign_keys = ON;");

            const string createDrugTable = """
                                           CREATE TABLE IF NOT EXISTS Drug (
                                               Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                               Name TEXT NOT NULL,
                                               Description TEXT,
                                               Usage TEXT,
                                               Specification TEXT
                                           );
                                           """;
            connection.Execute(createDrugTable);

            var createRuleTable = """
                                  CREATE TABLE IF NOT EXISTS DrugCalculationRule (
                                      Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                      DrugId INTEGER,
                                      Condition TEXT,
                                      Formula TEXT,
                                      Unit TEXT,
                                      Frequency TEXT,
                                      Route TEXT,
                                      FOREIGN KEY (DrugId) REFERENCES Drug(Id) ON DELETE CASCADE
                                  );
                                  """;
            connection.Execute(createRuleTable);
        }

        // 插入药物及其计算规则
        public void InsertDrug(Drug drug)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // 插入药物并返回生成的Id
            var drugId = connection.QuerySingle<int>(
                """
                INSERT INTO Drug (Name, Description, Usage, Specification)
                VALUES (@Name, @Description, @Usage, @Specification);
                SELECT last_insert_rowid();
                """,
                drug);

            // 插入关联的计算规则
            foreach (var rule in drug.CalculationRules)
            {
                rule.DrugId = drugId; // 设置规则的药物ID
                InsertCalculationRule(connection, rule);
            }
        }

        // 批量插入药物数据
        public void BulkInsertDrugs(DataTable dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
            {
                Console.WriteLine(@"DataTable为空，未执行插入操作。");
                return;
            }

            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // 构建插入SQL
                var columnNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                var parameterNames = string.Join(", ", dataTable.Columns.Cast<DataColumn>().Select(c => "@" + c.ColumnName));
                var insertSql = $"INSERT INTO Drug ({columnNames}) VALUES ({parameterNames});";
                Console.WriteLine($@"构造的SQL语句: {insertSql}");

                // 批量插入数据
                var parametersList = dataTable.Rows.Cast<DataRow>().Select(row =>
                {
                    var parameters = new DynamicParameters();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        parameters.Add("@" + column.ColumnName, row[column] == DBNull.Value ? null : row[column]);
                    }
                    return parameters;
                }).ToList();

                Console.WriteLine(@"执行批量插入。");
                connection.Execute(insertSql, parametersList, transaction: transaction);

                Console.WriteLine(@"提交事务。");
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"发生异常: " + ex.Message);
                Console.WriteLine(@"回滚事务。");
                transaction.Rollback();
                throw;
            }
        }

        // 更新药物及其计算规则
        public void UpdateDrug(Drug drug)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // 更新药物
            connection.Execute(
                @"UPDATE Drug SET Name = @Name, Description = @Description, Usage = @Usage, Specification = @Specification 
                  WHERE Id = @Id",
                drug);

            // 删除原有的计算规则
            connection.Execute("DELETE FROM DrugCalculationRule WHERE DrugId = @DrugId", new { DrugId = drug.Id });

            // 插入新的计算规则
            foreach (var rule in drug.CalculationRules)
            {
                rule.DrugId = drug.Id; // 设置规则的药物ID
                InsertCalculationRule(connection, rule);
            }
        }

        // 插入计算规则（内部方法）
        private static void InsertCalculationRule(IDbConnection connection, DrugCalculationRule rule)
        {
            const string sql = """
                               INSERT INTO DrugCalculationRule (DrugId, Condition, Formula, Unit, Frequency, Route)
                               VALUES (@DrugId, @Condition, @Formula, @Unit, @Frequency, @Route);
                               """;
            connection.Execute(sql, rule);
        }

        // 获取药物列表
        public IEnumerable<Drug> GetDrugs()
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // 获取所有药物
            var drugs = connection.Query<Drug>("SELECT * FROM Drug").ToList();

            // 获取每个药物的计算规则
            foreach (var drug in drugs)
            {
                drug.CalculationRules = new ObservableCollection<DrugCalculationRule>(GetCalculationRules(drug.Id).ToList());
            }

            return drugs;
        }

        // 获取药物的计算规则
        public IEnumerable<DrugCalculationRule> GetCalculationRules(int drugId)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            const string sql = "SELECT * FROM DrugCalculationRule WHERE DrugId = @DrugId";
            return connection.Query<DrugCalculationRule>(sql, new { DrugId = drugId }).ToList();
        }

        // 删除药物（级联删除其计算规则）
        public void DeleteDrug(int id)
        {
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            connection.Execute("DELETE FROM Drug WHERE Id = @Id", new { Id = id });
        }
    }
}
