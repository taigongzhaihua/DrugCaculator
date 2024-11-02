using DrugCaculator.Models;
using System.Collections.Generic;
using System.Data;

namespace DrugCaculator.Services
{
    public class DrugService
    {
        private readonly DbManager _dbManager = new();

        // 获取所有药物及其规则
        public IEnumerable<Drug> GetAllDrugs()
        {
            return _dbManager.GetDrugs();
        }

        // 添加药物及其规则
        public void AddDrug(Drug drug)
        {
            _dbManager.InsertDrug(drug);
        }
        // 批量添加药物数据
        public void AddDrugsFromTable(DataTable dataTable)
        {
            _dbManager.BulkInsertDrugs(dataTable);
        }
        // 更新药物及其规则
        public void UpdateDrug(Drug drug)
        {
            _dbManager.UpdateDrug(drug);
        }

        // 删除药物及其规则
        public void DeleteDrug(int drugId)
        {
            _dbManager.DeleteDrug(drugId);
        }
    }
}