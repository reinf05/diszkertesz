using diszkerteszClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace diszkerteszClient.Services
{
    public class PlantService
    {
        private HttpClient httpClient;
        private List<Plant> plants = new();
        private string baseURL = "http://192.168.1.151:5000/Plant/";

        public PlantService()
        {
            httpClient = new();
        }

        public async Task<List<Plant>> GetAllPlants()
        {
            if(plants.Count > 0)
            {
                return plants;
            }

            string URL = baseURL + "plants";
            var response = await httpClient.GetAsync(URL);
            if (response.IsSuccessStatusCode)
            {
                plants = await response.Content.ReadFromJsonAsync<List<Plant>>();
                return plants;
            }
            return null;
        }

        public async Task<FullPlant> GetFullPlantById(int id)
        {
            string URL = baseURL + "fullplants/" + id;
            var response = await httpClient.GetAsync(URL);
            if (response.IsSuccessStatusCode)
            {
                var plant = await response.Content.ReadFromJsonAsync<FullPlant>();
                return plant;
            }
            return null;
        }

        public async Task<Quiz> GetQuiz()
        {
            string URL = baseURL + "quiz";
            var response = await httpClient.GetAsync(URL);
            if (response.IsSuccessStatusCode)
            {
                var quiz = await response.Content.ReadFromJsonAsync<Quiz>();
                return quiz;
            }
            return null;
        }
    }
}
