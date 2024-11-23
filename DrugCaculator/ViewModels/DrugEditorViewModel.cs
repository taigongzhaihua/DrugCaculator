using DrugCalculator.Models;
using DrugCalculator.Services;
using DrugCalculator.Utilities.Commands;
using NLog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace DrugCalculator.ViewModels
{
    /// <summary>
    /// 药物编辑视图模型，用于管理药物和计算规则的新增、修改和删除操作。
    /// 实现了 INotifyPropertyChanged 接口以支持数据绑定。
    /// </summary>
    public class DrugEditorViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly DrugService _drugService; // 药物服务对象，用于操作数据库

        /// <summary>
        /// 请求关闭窗口的操作，通常由视图绑定。
        /// </summary>
        public Action CloseAction { get; set; }

        /// <summary>
        /// 指示当前操作是新增药物还是修改药物。
        /// </summary>
        public bool IsNew { get; }

        /// <summary>
        /// 当前编辑的药物对象。
        /// </summary>
        public Drug Drug { get; set; }

        private DrugCalculationRule _selectedRule;

        /// <summary>
        /// 当前选中的计算规则。
        /// </summary>
        public DrugCalculationRule SelectedRule
        {
            get => _selectedRule;
            set
            {
                _selectedRule = value;
                OnPropertyChanged(); // 通知绑定更新
                OnPropertyChanged(nameof(IsRuleSelected)); // 更新规则选择状态
                Logger.Debug($"选中的计算规则已更改: {_selectedRule?.Condition}");
            }
        }

        /// <summary>
        /// 当前是否选中了一个规则。
        /// </summary>
        public bool IsRuleSelected => SelectedRule != null;

        /// <summary>
        /// 构造函数，初始化药物编辑视图模型。
        /// </summary>
        /// <param name="drug">药物对象，如果为 null 则表示新增药物。</param>
        /// <param name="drugService">药物服务对象。</param>
        public DrugEditorViewModel(Drug drug, DrugService drugService)
        {
            Logger.Info("初始化 DrugEditorViewModel");
            _drugService = drugService;
            Drug = drug ?? new Drug { CalculationRules = [] }; // 如果传入 drug 为 null，则初始化一个新药物
            IsNew = drug == null; // 根据是否传入药物对象判断是新增还是修改
        }

        /// <summary>
        /// 默认构造函数，用于测试场景。
        /// </summary>
        public DrugEditorViewModel()
        {
            Logger.Info("初始化测试用 DrugEditorViewModel");
            Drug = new Drug { Name = "测试药物", CalculationRules = [] }; // 初始化测试药物
        }

        /// <summary>
        /// 添加规则命令。
        /// </summary>
        public ICommand AddRuleCommand => new RelayCommand(_ => AddRule());

        /// <summary>
        /// 添加新的计算规则到药物对象。
        /// </summary>
        private void AddRule()
        {
            Logger.Info("添加新的计算规则");
            // 创建一个默认的计算规则
            var newRule = new DrugCalculationRule
            {
                Condition = "Age > 10 year",
                Formula = "1",
                Unit = "mg",
                Frequency = "QD"
            };
            Drug.CalculationRules.Add(newRule); // 添加到药物的规则列表
            Logger.Debug($"添加了新的计算规则: {newRule.Condition}");
        }

        /// <summary>
        /// 删除规则命令。
        /// </summary>
        public ICommand DeleteRuleCommand => new RelayCommand(_ => DeleteRule());

        /// <summary>
        /// 删除选中的计算规则。
        /// </summary>
        private void DeleteRule()
        {
            if (SelectedRule == null)
            {
                Logger.Error("尝试删除规则时未选择任何规则");
                return;
            }

            var condition = SelectedRule.Condition;
            Logger.Info("删除选中的计算规则");
            Drug.CalculationRules.Remove(SelectedRule); // 从规则列表中移除选中的规则
            Logger.Debug($"已删除计算规则: {condition}");
            SelectedRule = null; // 清空选中的规则
            OnPropertyChanged(nameof(IsRuleSelected)); // 更新规则选择状态
            UpdateRuleList(); // 通知绑定更新规则列表
        }

        /// <summary>
        /// 保存药物命令。
        /// </summary>
        public ICommand SaveDrugCommand => new RelayCommand(_ => SaveDrug());

        /// <summary>
        /// 保存药物到数据库。
        /// </summary>
        private void SaveDrug()
        {
            if (string.IsNullOrWhiteSpace(Drug.Name))
            {
                Logger.Error("尝试保存药物时名称为空");
                MessageBox.Show("药物名称不能为空！"); // 提示用户药物名称不能为空
                return;
            }

            try
            {
                if (IsNew)
                {
                    Logger.Info("新增药物到数据库");
                    _drugService.AddDrug(Drug); // 调用服务新增药物
                    Logger.Info("药物新增成功");
                    MessageBox.Show("药物新增成功！");
                }
                else
                {
                    Logger.Info("修改药物到数据库");
                    _drugService.UpdateDrug(Drug); // 调用服务更新药物
                    Logger.Info("药物修改成功");
                    MessageBox.Show("药物修改成功！");
                }

                CloseAction?.Invoke(); // 调用关闭窗口的操作
            }
            catch (Exception ex)
            {
                Logger.Error($"保存药物时发生错误: {ex.Message}");
                MessageBox.Show($"保存药物时发生错误: {ex.Message}"); // 提示保存失败
            }
        }

        /// <summary>
        /// 更新规则列表。
        /// </summary>
        private void UpdateRuleList()
        {
            Logger.Debug("更新计算规则列表");
            OnPropertyChanged(nameof(Drug.CalculationRules)); // 通知绑定更新规则列表
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知属性变化。
        /// </summary>
        /// <param name="propertyName">发生变化的属性名称，默认为调用方法的名称。</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Logger.Debug($"属性 {propertyName} 发生变化。");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
