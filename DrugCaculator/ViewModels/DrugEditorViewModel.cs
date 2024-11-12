using DrugCaculator.Models;
using DrugCaculator.Services;
using DrugCaculator.Utilities.Commands;
using NLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DrugCaculator.ViewModels
{
    public class DrugEditorViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DrugService _drugService;

        // 定义关闭窗口的 Action
        public Action CloseAction { get; set; }

        // 当前操作类型：新增或修改
        public bool IsNew { get; }

        // 药物对象
        public Drug Drug { get; set; }

        // 选中的计算规则
        private DrugCalculationRule _selectedRule;
        public DrugCalculationRule SelectedRule
        {
            get => _selectedRule;
            set
            {
                _selectedRule = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRuleSelected));
                Logger.Debug($"选中的计算规则已更改: {_selectedRule?.Condition}");
            }
        }

        // 是否选中规则
        public bool IsRuleSelected => SelectedRule != null;

        // 构造函数
        public DrugEditorViewModel(Drug drug, DrugService drugService)
        {
            Logger.Info("初始化 DrugEditorViewModel");
            _drugService = drugService;
            Drug = drug ?? new Drug { CalculationRules = [] };
            IsNew = drug == null;
        }

        // 默认构造函数（测试用）
        public DrugEditorViewModel()
        {
            Logger.Info("初始化测试用 DrugEditorViewModel");
            Drug = new Drug { Name = "测试药物", CalculationRules = [] };
        }

        // 添加规则命令
        public ICommand AddRuleCommand => new RelayCommand(_ => AddRule());

        // 添加新的计算规则
        private void AddRule()
        {
            Logger.Info("添加新的计算规则");
            var newRule = new DrugCalculationRule { Condition = "Age > 10 year", Formula = "1", Unit = "mg", Frequency = "QD" };
            Drug.CalculationRules.Add(newRule);
            Logger.Debug($"添加了新的计算规则: {newRule.Condition}");
        }

        // 删除规则命令
        public ICommand DeleteRuleCommand => new RelayCommand(_ => DeleteRule());

        // 删除选中的计算规则
        private void DeleteRule()
        {
            if (SelectedRule == null)
            {
                Logger.Error("尝试删除规则时未选择任何规则");
                return;
            }

            Logger.Info("删除选中的计算规则");
            Drug.CalculationRules.Remove(SelectedRule);
            Logger.Debug($"已删除计算规则: {SelectedRule.Condition}");
            // 重置 SelectedRule
            SelectedRule = null;
            OnPropertyChanged(nameof(IsRuleSelected));
            UpdateRuleList();
        }

        // 保存药物命令
        public ICommand SaveDrugCommand => new RelayCommand(_ => SaveDrug());

        // 保存药物
        private void SaveDrug()
        {
            if (string.IsNullOrWhiteSpace(Drug.Name))
            {
                Logger.Error("尝试保存药物时名称为空");
                MessageBox.Show("药物名称不能为空！");
                return;
            }

            try
            {
                if (IsNew)
                {
                    Logger.Info("新增药物到数据库");
                    _drugService.AddDrug(Drug);
                    Logger.Info("药物新增成功");
                    MessageBox.Show("药物新增成功！");
                }
                else
                {
                    Logger.Info("修改药物到数据库");
                    _drugService.UpdateDrug(Drug);
                    Logger.Info("药物修改成功");
                    MessageBox.Show("药物修改成功！");
                }

                // 调用关闭窗口的 Action
                CloseAction?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"保存药物时发生错误: {ex.Message}");
                MessageBox.Show($"保存药物时发生错误: {ex.Message}");
            }
        }

        // 更新规则列表
        private void UpdateRuleList()
        {
            Logger.Debug("更新计算规则列表");
            OnPropertyChanged(nameof(Drug.CalculationRules));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // 属性变化通知
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Logger.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
