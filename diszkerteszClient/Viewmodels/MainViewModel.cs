using diszkerteszClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using diszkerteszClient.Services;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using diszkerteszClient.View;
using CommunityToolkit.Mvvm.ComponentModel;

namespace diszkerteszClient.Viewmodels
{
    public partial class MainViewModel : BaseViewModel
    {
        private PlantService plantService;
        private int _currentPage = 1;
        private bool _canLoadNextPage = true;
        public ObservableCollection<Plant> PlantList { get; } = new();
        private FullPlant fullPlant;
        private readonly string baseURL = "https://stdiszkerteszgerdev001.blob.core.windows.net/images/";

        [ObservableProperty] //Does not stall UI, but stalls not needed invocations of functions
        public bool isBusyMore = false;
        public MainViewModel(PlantService plantService)
        {
            this.Title = "Dísznövények";
            this.plantService = plantService;
        }

        [RelayCommand]
        async Task GoToDetailsAsync(Plant plant)
        {
            //AppShell.xaml.cs-ben regisztrálni kell a DetailPage-t routeként
            if (plant is null) return;
            await GetFullPlantAsync(plant.Id);
            if (fullPlant is null) return;
            await Shell.Current.GoToAsync(nameof(DetailPage), true, new Dictionary<string, object>
             {
                 {"fullPlant", fullPlant }
             });
        }

        public bool CanLoadMore => !IsBusy && !IsBusyMore && _canLoadNextPage; //Helper function
        //This will stop the user from scrolling more when there is data loading
        [RelayCommand(CanExecute = nameof(CanLoadMore))]
        async Task GetMorePageAsync()
        {
            if (IsBusy || IsBusyMore || !_canLoadNextPage)
            {
                return;
            }
            try
            {
                IsBusyMore = true;
                await LoadPageAsync();
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusyMore = false;
            }

        }


        [RelayCommand]
        async Task GetFirstPageAsync()
        {
            if (IsBusy)
            {
                return;
            }
            try
            {
                IsBusy = true;

                await LoadPageAsync();

                IsLoaded = true;

            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LoadPageAsync()
        {

            var page = await plantService.GetPlantPageAsync(_currentPage);

            foreach (var plant in page.Items)
            {
                string path = plant.Imagepath;
                plant.Imagepath = baseURL + path;
                PlantList.Add(plant);
            }
            _canLoadNextPage = page.HasNextPage;
            if (_canLoadNextPage)
            {
                _currentPage++;
            }
        }
        private async Task GetFullPlantAsync(int plantId)
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var plant = await plantService.GetFullPlantById(plantId);
                fullPlant = null!;

                if (plant == null)
                {
                    await Shell.Current.DisplayAlert("Error", "A növény nem található", "OK");
                    return;
                }

                fullPlant = new FullPlant()
                {
                    Id = plant.Id,
                    Namel = plant.Namel,
                    Nameh = plant.Nameh,
                    Type = plant.Type,
                    Imagepath = plant.Imagepath,
                    Description = plant.Description,
                    Usage = plant.Usage,
                    Pathogens = plant.Pathogens,
                    Propagation = plant.Propagation
                };
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}

