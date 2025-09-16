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

namespace diszkerteszClient.Viewmodels
{
    public partial class MainViewModel : BaseViewModel
    {
        private PlantService plantService;
        public ObservableCollection<Plant> PlantList { get; } = new();
        public MainViewModel(PlantService plantService)
        {
            this.Title = "Dísznövények";
            this.plantService = plantService;
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
    }
}
