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

namespace DrugCalculator.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public ObservableCollection<Drug> DrugsOnList { get; set; }
    private ObservableCollection<Drug> Drugs { get; }
    public DrugService DrugService { get; set; }
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

    public Drug SelectedDrug
    {
        get => _selectedDrug;
        set
        {
            _selectedDrug = value;
            CalculateDosage();
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsDrugSelected));
        }
    }

    public bool IsDrugSelected => SelectedDrug != null;

    private double _weight;

    public double Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            OnPropertyChanged();
            CalculateDosage();
        }
    }

    private int _age;

    public int Age
    {
        get => _age;
        set
        {
            _age = value;
            OnPropertyChanged();
            CalculateDosage();
        }
    }

    private bool _isChild = true;

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

    public bool IsAdult => !IsChild;
    private string _searchText;

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            SearchDrug();
        }
    }

    private string _ageUnit;

    public string AgeUnit
    {
        get => _ageUnit;
        set
        {
            _ageUnit = value;
            OnPropertyChanged();
            CalculateDosage();
        }
    }

    private CalculationResult _result;

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


    // 构造函数
    public MainWindowViewModel()
    {
        DrugService = new DrugService();
        Drugs = [];

        LoadDrugs(); // 数据，从数据库中加载
        InitCommands(); // 初始化命令
    }

    // 加载药物数据
    private void LoadDrugs()
    {
        Drugs.Clear();
        var drugList = DrugService.GetAllDrugs();
        var sortedDrugList = drugList.ToList().OrderBy(PinyinHelper.GetPinyin);
        foreach (var drug in sortedDrugList) Drugs.Add(drug);
        // 初始化 DrugsOnList 列表
        DrugsOnList = new ObservableCollection<Drug>(Drugs);
    }

    // 初始化命令
    private void InitCommands()
    {
        AddDrugCommand = new RelayCommand(AddDrug); // 创建新药物
        EditDrugCommand = new RelayCommand(EditDrug); // 编辑药物
        DeleteDrugCommand = new RelayCommand(DeleteDrug); // 删除药物
        AddDrugsFromExcelCommand = new RelayCommand(AddDrugsFromExcel); // 从 Excel 导入药物
        AiGenerateRuleCommand = new RelayCommand(GenerateRule); // 生成规则
        AiGenerateAllRulesCommand = new RelayCommand(GenerateAndSaveCalculationRulesForAllDrugsAsync); // 生成所有规则
        SetApiKeyCommand = new RelayCommand(SetApiKey); // 设置 API 密钥
        SettingCommand = new RelayCommand(SettingsOpen); // 打开设置窗口
        LogsCommand = new RelayCommand(ShowLogs);
    }

    private static void ShowLogs(object sender)
    {
        var apiKeySetter = new LogViewer
        {
            Owner = Application.Current.MainWindow
        };
        apiKeySetter.Show();
    }

    // 设置 API 密钥
    private static void SetApiKey(object sender)
    {
        var apiKeySetter = new ApiKeySetter
        {
            Owner = Window.GetWindow((sender as Button)!)
        };
        apiKeySetter.ShowDialog();
    }

    // 打开设置窗口
    private static void SettingsOpen(object sender)
    {
        var settings = new SettingsWindow
        {
            Owner = Application.Current.MainWindow
        };
        settings.ShowDialog();
    }


    // 生成规则
    private async void GenerateRule(object sender)
    {
        await GenerateAndSaveCalculationRulesAsync(SelectedDrug);
    }

    // 从 Excel 导入药物
    public void AddDrugsFromExcel(object sender)
    {
        // 创建文件选择对话框
        var openFileDialog = new OpenFileDialog
        {
            Filter = @"Excel Files|*.xls;*.xlsx",
            Title = @"选择一个 Excel 文件"
        };

        // 如果用户选择了文件
        if (openFileDialog.ShowDialog() != DialogResult.OK) return;
        var filePath = openFileDialog.FileName;
        try
        {
            // 创建 ExcelService 实例并读取 Excel 文件到 DataTable
            var dataTable = ExcelManager.Read(filePath);

            // 调用 DrugService.AddDrugsFromTable 方法，将数据导入数据库
            DrugService.AddDrugsFromTable(dataTable);
            Logger.Info("成功导入药品数据");
            CustomMessageBox.Show(@"数据导入成功", "成功", MsgBoxButtons.Ok, MsgBoxIcon.Success);
            LoadDrugs(); // 更新药物列表
            SearchDrug();
        }
        catch (Exception ex)
        {
            Logger.Error($@"导入药品数据时出错：{ex.Message}", ex);
            CustomMessageBox.Show($@"导入药品数据时出错：{ex.Message}", "错误", MsgBoxButtons.Ok, MsgBoxIcon.Error);
        }
    }

    private async void GenerateAndSaveCalculationRulesForAllDrugsAsync(object sender)
    {
        foreach (var drug in Drugs)
        {
            Console.WriteLine($@"正在生成【{drug.Name}】规则");
            await GenerateAndSaveCalculationRulesAsync(drug);
            Console.WriteLine($@"【{drug.Name}】生成完毕！");
        }

        Console.WriteLine(@"所有药物规则生成完毕！");
        MessageBox.Show(@"所有药物规则生成完毕！");
    }

    private async Task GenerateAndSaveCalculationRulesAsync(Drug drug)
    {
        try
        {
            // 使用 DeepSeekService 生成药物的计算规则
            var generatedRules = await DeepSeekService.GenerateDrugCalculationRulesAsync(drug);

            // 将生成的计算规则存储在药物对象的 CalculationRules 中
            if (generatedRules != null && generatedRules.Count != 0)
            {
                drug.CalculationRules = new ObservableCollection<DrugCalculationRule>(generatedRules);
                Console.WriteLine($@"共生成{drug.CalculationRules.Count}条规则");
                // 将更新后的药物对象存入数据库
                DrugService.UpdateDrug(drug);
            }
            else
            {
                Console.WriteLine(@"未生成任何计算规则。");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"生成和保存计算规则时发生错误: {ex.Message}");
        }
    }

    private void DeleteDrug(object parameter)
    {
        var result = CustomMessageBox.Show(Window.GetWindow((parameter as Button)!),
            $"是否删除“{SelectedDrug.Name}”？",
            "是否删除？",
            MsgBoxButtons.YesNo,
            MsgBoxIcon.Information);

        if (result != MessageBoxResult.Yes) return;
        DrugService.DeleteDrug(SelectedDrug.Id);
        MessageBox.Show(@"删除成功！");
        LoadDrugs(); // 更新药物列表
        SearchDrug();
    }

    private void AddDrug(object parameter)
    {
        var drugEditor = new DrugEditor(null, DrugService)
        {
            Owner = Window.GetWindow((parameter as Button)!)
        }; // 创建 DrugEditor 实例
        drugEditor.ShowDialog();
        LoadDrugs(); // 更新药物列表
        SearchDrug();
    }

    private void EditDrug(object parameter)
    {
        var drugEditor = new DrugEditor(SelectedDrug, DrugService)
        {
            Owner = Window.GetWindow((parameter as Button)!)
        };
        drugEditor.ShowDialog();
        LoadDrugs(); // 更新药物列表
        SearchDrug();
    }

    private void SearchDrug()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // 如果搜索框为空，则显示全部药物
            DrugsOnList = Drugs;
        }
        else
        {
            // 筛选药物列表，支持中文查找和首字母查找
            var filteredDrugs = new ObservableCollection<Drug>(Drugs.Where(drug =>
                drug.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
                PinyinHelper.GetFirstLetter(drug).Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)).ToList());
            DrugsOnList = filteredDrugs;
        }

        OnPropertyChanged(nameof(DrugsOnList));
    }

    public void CalculateDosage()
    {
        // 实现计算剂量逻辑
        if (SelectedDrug == null || !(Weight > 0)) return;
        // 假设实现剂量计算
        var drug = SelectedDrug;
        if (drug == null) return;
        var rules = drug.CalculationRules;
        var unit = AgeUnit switch
        {
            "岁" => "year",
            "月" => "month",
            _ => "year"
        };
        var jsonResult = CalculateService.CalculateRules(rules, Age, unit, Weight);

        // 反序列化 JSON 字符串
        Result = JsonConvert.DeserializeObject<CalculationResult>(jsonResult);
        OnPropertyChanged(nameof(Result));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}