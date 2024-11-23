using DrugCalculator.DataAccess;
using DrugCalculator.Models;
using DrugCalculator.Services;
using DrugCalculator.Utilities.Commands;
using DrugCalculator.Utilities.Helpers;
using DrugCalculator.View.Components;
using DrugCalculator.View.Windows;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;

#pragma warning disable CA1416

namespace DrugCalculator.ViewModels
{
    /// <summary>
    /// 主窗口的视图模型，负责管理药物数据、用户交互逻辑和命令绑定。
    /// 实现了 INotifyPropertyChanged 接口以支持数据绑定。
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 当前显示在界面上的药物列表。
        /// </summary>
        public ObservableCollection<Drug> DrugsOnList { get; set; }

        /// <summary>
        /// 从数据库加载的完整药物列表。
        /// </summary>
        private ObservableCollection<Drug> Drugs { get; }

        /// <summary>
        /// 药物服务对象，用于操作药物数据。
        /// </summary>
        public DrugService DrugService { get; set; }

        // 定义所有命令，用于绑定到界面按钮
        public ICommand AddDrugCommand { get; set; }
        public ICommand EditDrugCommand { get; set; }
        public ICommand DeleteDrugCommand { get; set; }
        public ICommand AddDrugsFromExcelCommand { get; set; }
        public ICommand AiGenerateRuleCommand { get; set; }
        public ICommand AiGenerateAllRulesCommand { get; set; }
        public ICommand SettingCommand { get; set; }
        public ICommand SetApiKeyCommand { get; set; }
        public ICommand LogsCommand { get; set; }

        private string _dosage;

        /// <summary>
        /// 当前计算出的剂量，展示在界面上。
        /// </summary>
        public string Dosage
        {
            get => _dosage;
            set
            {
                _dosage = value;
                OnPropertyChanged();
            }
        }

        private Drug _selectedDrug;

        /// <summary>
        /// 当前选中的药物。
        /// </summary>
        public Drug SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                _selectedDrug = value;
                CalculateDosage(); // 当选中药物变化时重新计算剂量
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDrugSelected));
            }
        }

        /// <summary>
        /// 指示是否有选中的药物。
        /// </summary>
        public bool IsDrugSelected => SelectedDrug != null;

        private double _weight;

        /// <summary>
        /// 当前用户输入的体重。
        /// </summary>
        public double Weight
        {
            get => _weight;
            set
            {
                _weight = value;
                OnPropertyChanged();
                CalculateDosage(); // 当体重变化时重新计算剂量
            }
        }

        private int _age;

        /// <summary>
        /// 当前用户输入的年龄。
        /// </summary>
        public int Age
        {
            get => _age;
            set
            {
                _age = value;
                OnPropertyChanged();
                CalculateDosage(); // 当年龄变化时重新计算剂量
            }
        }

        private bool _isChild = true;

        /// <summary>
        /// 指示用户是否为儿童。
        /// </summary>
        public bool IsChild
        {
            get => _isChild;
            set
            {
                _isChild = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAdult));
            }
        }

        /// <summary>
        /// 指示用户是否为成人。
        /// </summary>
        public bool IsAdult => !IsChild;

        private string _searchText;

        /// <summary>
        /// 用户输入的搜索文本，用于筛选药物列表。
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                SearchDrug(); // 当搜索文本变化时更新药物列表
            }
        }

        private string _ageUnit;

        /// <summary>
        /// 当前用户输入的年龄单位（岁或月）。
        /// </summary>
        public string AgeUnit
        {
            get => _ageUnit;
            set
            {
                _ageUnit = value;
                OnPropertyChanged();
                CalculateDosage(); // 当年龄单位变化时重新计算剂量
            }
        }

        private CalculationResult _result;

        /// <summary>
        /// 当前的计算结果。
        /// </summary>
        public CalculationResult Result
        {
            get => _result;
            set
            {
                _result = value;
                OnPropertyChanged();
                Dosage = Result.Dosage switch
                {
                    0 => "不可使用",
                    -1 => "遵医嘱",
                    _ => Result.Dosage + Result.Unit
                };
                OnPropertyChanged(nameof(Dosage));
            }
        }

        /// <summary>
        /// 构造函数，初始化视图模型并加载数据。
        /// </summary>
        public MainWindowViewModel()
        {
            DrugService = new DrugService();
            Drugs = [];

            LoadDrugs(); // 加载药物数据
            InitCommands(); // 初始化命令
        }

        /// <summary>
        /// 从数据库加载药物列表。
        /// </summary>
        private void LoadDrugs()
        {
            Drugs.Clear();
            var drugList = DrugService.GetAllDrugs(); // 获取药物列表
            var sortedDrugList = drugList.ToList().OrderBy(PinyinHelper.GetPinyin); // 按拼音排序
            foreach (var drug in sortedDrugList) Drugs.Add(drug);
            DrugsOnList = new ObservableCollection<Drug>(Drugs); // 初始化 DrugsOnList
        }

        /// <summary>
        /// 初始化命令。
        /// </summary>
        private void InitCommands()
        {
            AddDrugCommand = new RelayCommand(AddDrug);
            EditDrugCommand = new RelayCommand(EditDrug);
            DeleteDrugCommand = new RelayCommand(DeleteDrug);
            AddDrugsFromExcelCommand = new RelayCommand(AddDrugsFromExcel);
            AiGenerateRuleCommand = new RelayCommand(GenerateRule);
            AiGenerateAllRulesCommand = new RelayCommand(GenerateAndSaveCalculationRulesForAllDrugsAsync);
            SetApiKeyCommand = new RelayCommand(SetApiKey);
            SettingCommand = new RelayCommand(SettingsOpen);
            LogsCommand = new RelayCommand(ShowLogs);
        }

        /// <summary>
        /// 展示日志窗口。
        /// </summary>
        private static void ShowLogs(object sender)
        {
            var logViewer = new LogViewer
            {
                Owner = Application.Current.MainWindow
            };
            logViewer.Show();
        }

        /// <summary>
        /// 设置 API 密钥。
        /// </summary>
        private static void SetApiKey(object sender)
        {
            var apiKeySetter = new ApiKeySetter
            {
                Owner = Window.GetWindow((sender as Button)!)
            };
            apiKeySetter.ShowDialog();
        }

        /// <summary>
        /// 打开设置窗口。
        /// </summary>
        private static void SettingsOpen(object sender)
        {
            var settings = new SettingsWindow
            {
                Owner = Application.Current.MainWindow
            };
            settings.ShowDialog();
        }

        /// <summary>
        /// 生成规则。
        /// </summary>
        private async void GenerateRule(object sender)
        {
            await GenerateAndSaveCalculationRulesAsync(SelectedDrug);
        }

        /// <summary>
        /// 从 Excel 文件导入药物。
        /// </summary>
        public void AddDrugsFromExcel(object sender)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = @"Excel Files|*.xls;*.xlsx",
                Title = @"选择一个 Excel 文件"
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return;
            var filePath = openFileDialog.FileName;

            try
            {
                var dataTable = ExcelManager.Read(filePath);
                DrugService.AddDrugsFromTable(dataTable);
                Logger.Info("成功导入药品数据");
                CustomMessageBox.Show("数据导入成功", "成功", MsgBoxButtons.Ok, MsgBoxIcon.Success);
                LoadDrugs();
                SearchDrug();
            }
            catch (Exception ex)
            {
                Logger.Error($"导入药品数据时出错：{ex.Message}", ex);
                CustomMessageBox.Show($"导入药品数据时出错：{ex.Message}", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
            }
        }

        /// <summary>
        /// 为所有药物生成计算规则。
        /// </summary>
        private async void GenerateAndSaveCalculationRulesForAllDrugsAsync(object sender)
        {
            foreach (var drug in Drugs)
            {
                await GenerateAndSaveCalculationRulesAsync(drug);
            }

            MessageBox.Show("所有药物规则生成完毕！");
        }

        /// <summary>
        /// 为指定药物生成和保存计算规则。
        /// </summary>
        private async Task GenerateAndSaveCalculationRulesAsync(Drug drug)
        {
            try
            {
                var generatedRules = await DeepSeekService.GenerateDrugCalculationRulesAsync(drug);

                if (generatedRules != null && generatedRules.Count != 0)
                {
                    drug.CalculationRules = new ObservableCollection<DrugCalculationRule>(generatedRules);
                    DrugService.UpdateDrug(drug);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"生成和保存计算规则时发生错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除选中的药物。
        /// </summary>
        private void DeleteDrug(object parameter)
        {
            var result = CustomMessageBox.Show(Window.GetWindow((parameter as Button)!),
                $"是否删除“{SelectedDrug.Name}”？",
                "是否删除？",
                MsgBoxButtons.YesNo,
                MsgBoxIcon.Information);

            if (result != MessageBoxResult.Yes) return;
            DrugService.DeleteDrug(SelectedDrug.Id);
            MessageBox.Show("删除成功！");
            LoadDrugs();
            SearchDrug();
        }

        /// <summary>
        /// 添加新药物。
        /// </summary>
        private void AddDrug(object parameter)
        {
            var drugEditor = new DrugEditor(null, DrugService)
            {
                Owner = Window.GetWindow((parameter as Button)!)
            };
            drugEditor.ShowDialog();
            LoadDrugs();
            SearchDrug();
        }

        /// <summary>
        /// 编辑选中的药物。
        /// </summary>
        private void EditDrug(object parameter)
        {
            var drugEditor = new DrugEditor(SelectedDrug, DrugService)
            {
                Owner = Window.GetWindow((parameter as Button)!)
            };
            drugEditor.ShowDialog();
            LoadDrugs();
            SearchDrug();
        }

        /// <summary>
        /// 根据用户输入的搜索文本筛选药物列表。
        /// </summary>
        private void SearchDrug()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DrugsOnList = Drugs;
            }
            else
            {
                var filteredDrugs = new ObservableCollection<Drug>(Drugs.Where(drug =>
                    drug.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
                    PinyinHelper.GetFirstLetter(drug).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)));
                DrugsOnList = filteredDrugs;
            }

            OnPropertyChanged(nameof(DrugsOnList));
        }

        /// <summary>
        /// 根据当前药物、体重和年龄计算剂量。
        /// </summary>
        public void CalculateDosage()
        {
            if (SelectedDrug == null || Weight <= 0) return;

            var unit = AgeUnit switch
            {
                "岁" => "year",
                "月" => "month",
                _ => "year"
            };
            var jsonResult = CalculateService.CalculateRules(SelectedDrug.CalculationRules, Age, unit, Weight);

            Result = JsonConvert.DeserializeObject<CalculationResult>(jsonResult);
            OnPropertyChanged(nameof(Result));
        }

        /// <summary>
        /// 属性变化通知事件，当绑定的属性值发生变化时触发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 通知绑定系统某个属性值已发生变化。
        /// </summary>
        /// <param name="name">发生变化的属性名称。</param>
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
