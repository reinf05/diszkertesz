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

namespace diszkerteszClient.Viewmodels
{
    public partial class MainViewModel : BaseViewModel
    {
        private PlantService plantService;
        public ObservableCollection<Plant> PlantList { get; } = new();
        private FullPlant fullPlant;
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
            await Shell.Current.GoToAsync(nameof(DetailPage), true, new Dictionary<string, object>
             {
                 {"fullPlant", fullPlant }
             });
        }

        [RelayCommand]
        async Task GetPlantsAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                var plants = await plantService.GetAllPlants();

                if (PlantList.Count != 0)
                {
                    PlantList.Clear();
                }

                string baseURL = "http://192.168.1.151:5000/images/";

                foreach(var plant in plants)
                {
                    string path = plant.Imagepath;
                    plant.Imagepath = baseURL + path;
                    PlantList.Add(plant);
                }
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

                if (plant == null)
                {
                    await Shell.Current.DisplayAlert("Error", "No plant found.", "OK");
                    return;
                }

                string baseURL = "http://192.168.1.151:5000/images/";
                fullPlant = new FullPlant()
                {
                    Id = plant.Id,
                    Namel = plant.Namel,
                    Nameh = plant.Nameh,
                    Type = plant.Type,
                    Imagepath = baseURL + plant.Imagepath,
                    Description = plant.Description,
                    Usage = plant.Usage,
                    Pathogens = plant.Pathogens,
                    Propagation = plant.Propagation
                };
                return;
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

