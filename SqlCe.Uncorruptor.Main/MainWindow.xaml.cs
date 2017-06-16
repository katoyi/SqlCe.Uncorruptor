using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SqlCe.Uncorruptor.Main
{
    public class CommandHandler : ICommand
    {
        private readonly Action _action;
        private readonly bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            Check.IsEnabled = false;
        }

        private Window _window;
        private string _fileName;
        private string _passWordContent;
        private bool _supprimerLigneCorrompues;
        private bool _reparerLigneCorrompues;
        private bool _reparerTouteLigneCorrompues;
        private bool _reparerToutOuRien;
        private ICommand _onChoiceDone;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged();
                Check.IsEnabled = !string.IsNullOrEmpty(FileName);
            }
        }

        public string PassWordContent
        {
            get { return _passWordContent; }

            set
            {
                _passWordContent = value;
                OnPropertyChanged();
            }
        }

        public bool ReparerToutOuRien
        {
            get { return _reparerToutOuRien; }

            set
            {
                _reparerToutOuRien = value;
                OnPropertyChanged();
            }
        }

        public bool ReparerTouteLigneCorrompues
        {
            get { return _reparerTouteLigneCorrompues; }

            set
            {
                _reparerTouteLigneCorrompues = value;
                OnPropertyChanged();
            }
        }

        public bool ReparerLigneCorrompues
        {
            get { return _reparerLigneCorrompues; }

            set
            {
                _reparerLigneCorrompues = value;
                OnPropertyChanged();
            }
        }

        public bool SupprimerLigneCorrompues
        {
            get { return _supprimerLigneCorrompues; }

            set
            {
                _supprimerLigneCorrompues = value;
                OnPropertyChanged();
            }
        }

        public ICommand OnChoiceDone => _onChoiceDone ?? (_onChoiceDone = new CommandHandler(OnChoice, true));

        private void OnChoice()
        {
            if (_window != null && (SupprimerLigneCorrompues || ReparerLigneCorrompues || ReparerToutOuRien || ReparerTouteLigneCorrompues))
                _window.Close();
        }

        private void LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Fichiers SqlCE (*.sdf)|*.sdf",
                Multiselect = false
            };

            if (ofd.ShowDialog() ?? false)
            {
                FileName = ofd.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var checker = new CheckDataBase(FileName, PassWordContent);
            if (!checker.Check())
            {
                var confirmation =
                    MessageBox.Show(this, "La base de données est corrompue, voulez vous tenter une réparation?",
                        "Information", MessageBoxButton.YesNo);
                if (confirmation == MessageBoxResult.No)
                {
                    return;
                }
                if (confirmation == MessageBoxResult.Yes)
                {
                    _window = new Window
                    {
                        Title = "Option de réparation",
                        Content = new CorrectionOptions { DataContext = this },
                        Height = 200,
                        Width = 450,
                        ResizeMode = ResizeMode.NoResize,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    _window.ShowDialog();
                    CheckDataBase.LocalRepairOption? option = null;
                    if (ReparerLigneCorrompues)
                        option = CheckDataBase.LocalRepairOption.RecoverCorruptedRows;
                    if (ReparerToutOuRien)
                        option = CheckDataBase.LocalRepairOption.RecoverAllOrFail;
                    if (ReparerTouteLigneCorrompues)
                        option = CheckDataBase.LocalRepairOption.RecoverAllPossibleRows;
                    if (SupprimerLigneCorrompues)
                        option = CheckDataBase.LocalRepairOption.DeleteCorruptedRows;

                    checker.Correct(option);
                }
            }
            else
            {
                MessageBox.Show("Aucun problème détecter");
            }
        }



        #region NotifProp
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}
