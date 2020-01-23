using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ImoutoRebirth.Navigator.Commands;
using MahApps.Metro;

namespace ImoutoRebirth.Navigator.ViewModel
{
    class SettingsVM : VMBase
    {
        #region Subclasses

        public class AccentColorMenuData
        {
            public string Name { get; set; }

            public Brush ColorBrush { get; set; }

            public void ChangeAccent()
            {
                ThemeManager.ChangeThemeColorScheme(Application.Current, Name);
                Settings.Default.AccentColorName = Name;
            }
        }

        #endregion Subclasses

        #region Fields

        private AccentColorMenuData _selectedAccentColor;
        private int _selectedTheme;
        private ICommand _saveCommand;

        #endregion Fileds

        #region Constructors

        public SettingsVM()
        {
            AccentColors = ThemeManager.ColorSchemes.Select(a => new AccentColorMenuData
                                                            {
                                                                Name = a.Name,
                                                                ColorBrush = a.ShowcaseBrush
                                                            }).ToList();
            SelectedAccentColor = AccentColors.First(x => x.Name == Settings.Default.AccentColorName);

            SelectedIndexTheme = Settings.Default.ThemeIndex;

            ShowPreviewOnSelect = Settings.Default.ActivatePreviewOnSelect;
        }

        #endregion Constructors

        #region Properties

        public bool ShowPreviewOnSelect
        {
            get
            {
                return Settings.Default.ActivatePreviewOnSelect;
            }
            set
            {
                Settings.Default.ActivatePreviewOnSelect = value;
                OnPropertyChanged(() => ShowPreviewOnSelect);
                OnShowPreviewOnSelectChanged();
            }
        }

        public List<AccentColorMenuData> AccentColors { get; }

        public AccentColorMenuData SelectedAccentColor
        {
            get
            {
                return _selectedAccentColor;
            }
            set
            {
                _selectedAccentColor = value;
                _selectedAccentColor.ChangeAccent();
            }
        }

        /// <summary>
        ///     Index of the selected theme. 0 - light, 1 - dark.
        /// </summary>
        public int SelectedIndexTheme
        {
            get
            {
                return _selectedTheme;
            }
            set
            {
                _selectedTheme = value;
                var theme = ThemeManager.DetectTheme(Application.Current);
                switch (value)
                {
                    case 1:
                        ThemeManager.ChangeThemeBaseColor(Application.Current, ThemeManager.ColorSchemes.Last().Name);
                        Settings.Default.ThemeIndex = 1;
                        break;
                    default:
                        ThemeManager.ChangeThemeBaseColor(Application.Current, ThemeManager.ColorSchemes.First().Name);
                        Settings.Default.ThemeIndex = 0;
                        break;
                }
            }
        }

        #endregion Properties

        #region Commands

        public ICommand SaveCommand
        {
            get
            {
                return _saveCommand ?? (_saveCommand = new RelayCommand(x => Save()));
            }
        }

        private void Save()
        {
            Settings.Default.Save();
        }

        #endregion Commands

        #region Events

        public event EventHandler ShowPreviewOnSelectChanged;

        private void OnShowPreviewOnSelectChanged()
        {
            var handler = ShowPreviewOnSelectChanged;
            handler?.Invoke(this, new EventArgs());
        }

        #endregion Events
    }
}
