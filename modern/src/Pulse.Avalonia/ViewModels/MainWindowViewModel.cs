using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pulse.Base;

namespace Pulse.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _status = "Ready";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _query = "nature";
        public string Query
        {
            get => _query;
            set { _query = value; OnPropertyChanged(); }
        }

        public List<Picture> Pictures { get; set; } = new();

        public async Task LoadWallhavenAsync()
        {
            Status = "Loading Wallhaven...";
            try
            {
                var settings = new wallbase.WallbaseImageSearchSettings
                {
                    Query = Query,
                    WG = true,
                    W = true,
                    SFW = true,
                    OB = "relevance",
                    OBD = "desc"
                };

                var provider = new wallbase.Provider();
                provider.Initialize(null);

                var search = new PictureSearch
                {
                    SearchProvider = new ActiveProviderInfo { ProviderConfig = settings.Save(), ProviderInstanceID = Guid.NewGuid() },
                    MaxPictureCount = 10,
                    PageToRetrieve = 1,
                    BannedURLs = new List<string>(),
                    SaveFolder = Path.Combine(Path.GetTempPath(), "PulseAvalonia"),
                    PreviewOnly = true
                };

                Directory.CreateDirectory(search.SaveFolder);
                var list = provider.GetPictures(search);
                Pictures = list.Pictures.Take(10).ToList();
                Status = $"Loaded {Pictures.Count} from Wallhaven";
                OnPropertyChanged(nameof(Pictures));
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }

        public async Task LoadBingAsync()
        {
            Status = "Loading Bing...";
            try
            {
                var provider = new BingWallpaper.Provider();
                provider.Initialize(null);

                var search = new PictureSearch
                {
                    MaxPictureCount = 8,
                    BannedURLs = new List<string>(),
                    SaveFolder = Path.Combine(Path.GetTempPath(), "PulseAvalonia"),
                    PreviewOnly = true,
                    SearchProvider = new ActiveProviderInfo { ProviderConfig = "", ProviderInstanceID = Guid.NewGuid() }
                };

                Directory.CreateDirectory(search.SaveFolder);
                var list = provider.GetPictures(search);
                Pictures = list.Pictures.ToList();
                Status = $"Loaded {Pictures.Count} from Bing (no key needed)";
                OnPropertyChanged(nameof(Pictures));
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }
    }

    public class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
