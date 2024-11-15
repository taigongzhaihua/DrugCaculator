using System.ComponentModel;

namespace DrugCalculator.Models;

public class ConditionRow : INotifyPropertyChanged
{
    private string _conditionType = "年龄";
    private string _comparison = "小于";
    private string _value = "1";
    private string _unit = "岁";
    private string _logic = "且";
    private bool _showLogic;

    public string ConditionType
    {
        get => _conditionType;
        set
        {
            if (_conditionType == value) return;
            _conditionType = value;
            OnPropertyChanged(nameof(ConditionType));
        }
    }

    public string Comparison
    {
        get => _comparison;
        set
        {
            if (_comparison == value) return;
            _comparison = value;
            OnPropertyChanged(nameof(Comparison));
        }
    }

    public string Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged(nameof(Value));
        }
    }

    public string Unit
    {
        get => _unit;
        set
        {
            if (_unit == value) return;
            _unit = value;
            OnPropertyChanged(nameof(Unit));
        }
    }

    public string Logic
    {
        get => _logic;
        set
        {
            if (_logic == value) return;
            _logic = value;
            OnPropertyChanged(nameof(Logic));
        }
    }

    public bool ShowLogic
    {
        get => _showLogic;
        set
        {
            if (_showLogic == value) return;
            _showLogic = value;
            OnPropertyChanged(nameof(ShowLogic));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return $"{ConditionType} {Comparison} {Value} {Unit}".Trim();
    }
}