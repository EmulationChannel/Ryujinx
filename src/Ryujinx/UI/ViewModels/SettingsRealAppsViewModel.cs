using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using Gommon;
using Ryujinx.Ava.Utilities.Configuration;
using System;
using System.Globalization;

namespace Ryujinx.Ava.UI.ViewModels
{
    public partial class SettingsRealAppsViewModel : BaseModel
    {
        private readonly SettingsViewModel _baseViewModel;

        public SettingsRealAppsViewModel() {}
        
        public SettingsRealAppsViewModel(SettingsViewModel settingsVm)
        {
            _baseViewModel = settingsVm;
        }
    }
}
