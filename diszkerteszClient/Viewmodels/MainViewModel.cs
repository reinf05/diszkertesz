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
        private readonly string baseURL = "https://stdiszkerteszgerdev001.blob.core.windows.net/images/";
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

                foreach(var plant in plants)
                {
                    string path = plant.Imagepath;
                    plant.Imagepath = baseURL + path;
                    PlantList.Add(plant);
                }
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
                    Imagepath = baseURL + plant.Imagepath,
                    Description = plant.Description,
                    Usage = plant.Usage,
                    Pathogens = plant.Pathogens,
                    Propagation = plant.Propagation
                };
                //try
                //{
                //    using var http = new HttpClient();
                //    string url = $"{fullPlant.Imagepath}1.jpeg";
                //    var res = await http.GetAsync(url);
                //    Console.WriteLine($"Status: {res.StatusCode}");
                //    var contentType = res.Content.Headers.ContentType?.ToString();
                //    Console.WriteLine($"Content-Type: {contentType}");
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine(ex.Message);
                //}
                //return;
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

