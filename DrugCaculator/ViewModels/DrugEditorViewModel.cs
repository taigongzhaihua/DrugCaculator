using DrugCaculator.Models;
using DrugCaculator.Services;
using DrugCaculator.Utilities.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DrugCaculator.ViewModels
{
    public class DrugEditorViewModel : INotifyPropertyChanged
    {
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
            }
        }

        // 是否选中规则
        public bool IsRuleSelected => SelectedRule != null;

        // 构造函数
        public DrugEditorViewModel(Drug drug, DrugService drugService)
        {
            _drugService = drugService;
            Drug = drug ?? new Drug { CalculationRules = [] };
            IsNew = drug == null;
        }

        public DrugEditorViewModel()
        {
            Drug = new Drug { Name = "测试药物", CalculationRules = [] };
        }


        // 添加规则命令
        public ICommand AddRuleCommand => new RelayCommand(param => AddRule());
        private void AddRule()
        {
            var newRule = new DrugCalculationRule { Condition = "Age > 10 year", Formula = "1", Unit = "mg", Frequency = "QD" };
            Drug.CalculationRules.Add(newRule);
        }


        // 删除规则命令
        public ICommand DeleteRuleCommand => new RelayCommand(param => DeleteRule());
        private void DeleteRule()
        {
            if (SelectedRule == null) return;

            // 从集合中移除选中的规则
            Drug.CalculationRules.Remove(SelectedRule);
            // 重置 SelectedRule
            // SelectedRule = null;
            OnPropertyChanged(nameof(SelectedRule));
            OnPropertyChanged(nameof(IsRuleSelected));
            UpdateRuleList();
        }

        // 保存药物命令
        public ICommand SaveDrugCommand => new RelayCommand(param => SaveDrug());
        private void SaveDrug()
        {
            if (string.IsNullOrWhiteSpace(Drug.Name))
            {
                MessageBox.Show("药物名称不能为空！");
                return;
            }

            if (IsNew)
            {
                // 新增药物到数据库
                _drugService.AddDrug(Drug);
                MessageBox.Show("药物新增成功！");
            }
            else
            {
                // 修改药物到数据库
                _drugService.UpdateDrug(Drug);
                MessageBox.Show("药物修改成功！");
            }

            // 调用关闭窗口的 Action
            CloseAction?.Invoke();
        }

        private void UpdateRuleList()
        {
            OnPropertyChanged(nameof(Drug.CalculationRules));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
