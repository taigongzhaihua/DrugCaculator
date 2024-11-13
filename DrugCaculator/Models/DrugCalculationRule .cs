using System.ComponentModel;

namespace DrugCalculator.Models
{
    public class DrugCalculationRule : INotifyPropertyChanged
    {
        private int _id;
        private int _drugId;
        private string _condition;
        private string _formula;
        private string _unit;
        private string _frequency;
        private string _route;
        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get => _id;
            set
            {
                if (_id == value) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public int DrugId
        {
            get => _drugId;
            set
            {
                if (_drugId == value) return;
                _drugId = value;
                OnPropertyChanged(nameof(DrugId));
            }
        }

        public string Condition
        {
            get => _condition;
            set
            {
                if (_condition == value) return;
                _condition = value;
                OnPropertyChanged(nameof(Condition));
            }
        }

        public string Formula
        {
            get => _formula;
            set
            {
                if (_formula == value) return;
                _formula = value;
                OnPropertyChanged(nameof(Formula));
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

        public string Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency == value) return;
                _frequency = value;
                OnPropertyChanged(nameof(Frequency));
            }
        }

        public string Route
        {
            get => _route;
            set
            {
                if (_route == value) return;
                _route = value;
                OnPropertyChanged(nameof(Route));
            }
        }
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
