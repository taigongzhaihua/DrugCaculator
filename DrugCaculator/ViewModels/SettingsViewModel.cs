using DrugCaculator.Utilities.Commands;
using DrugCaculator.View.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DrugCaculator.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _isCloseSetting;
        public string IsCloseSetting
        {
            get => _isCloseSetting;
            set
            {
                if (_isCloseSetting == value) return;
                _isCloseSetting = value;
                OnPropertyChanged();
                SaveSettings();
            }
        }

        public ICommand ConfigureApiKeyCommand { get; }

        public SettingsViewModel()
        {
            LoadSettings();
            ConfigureApiKeyCommand = new RelayCommand(OpenApiKeyDialog);
        }

        private void LoadSettings()
        {
            IsCloseSetting = Properties.Settings.Default.IsClose;
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.IsClose = IsCloseSetting;
            Properties.Settings.Default.Save();
        }

        private static void OpenApiKeyDialog(object obj)
        {
            var apiKeySetter = new ApiKeySetter
            {
                Owner = Window.GetWindow((obj as Button)!)
            };
            apiKeySetter.ShowDialog();
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