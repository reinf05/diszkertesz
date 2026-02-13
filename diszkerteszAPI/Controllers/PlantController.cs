using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using diszkerteszAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Web;
using static System.Net.Mime.MediaTypeNames;

namespace diszkerteszAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly int pageSize = 10;

        private readonly diszkerteszDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly HttpClient _httpClient;

        private readonly string? plantnetApiKey;
        private readonly string? perApiKey;
        private readonly string sparqlEndpoint = "https://query.wikidata.org/sparql";

        public PlantController(diszkerteszDbContext context, BlobServiceClient BlobServiceClient, IHttpClientFactory httpClientfactory)
        {
            _blobServiceClient = BlobServiceClient;
            _context = context;
            _httpClient = httpClientfactory.CreateClient();
            plantnetApiKey = Environment.GetEnvironmentVariable("API-KEY");
            perApiKey = Environment.GetEnvironmentVariable("PER-API-KEY");
        }


        [HttpGet("plants/{pageNum}")]
        public async Task<Page<Plant>> GetPlantByPage(int pageNum = 1)
        {
            if (pageNum < 1)
            {
                pageNum = 1;
            }
            int totalCount = await _context.Plants.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (pageNum > totalPages) pageNum = totalPages;

            var plants = await _context.Plants
                .OrderBy(p => p.ID)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new Page<Plant>()
            {
                Items = plants,
                PageNumber = pageNum,
                TotalPages = totalPages,
                TotalCount = totalCount
            };
        }


        [HttpGet("details/{id}")]
        public async Task<ActionResult<Detail>> GetDetailById(int id)
        {
            return await _context.Details.FindAsync(id);
        }

        [HttpGet("fullplants/{id}")]
        public async Task<ActionResult<Fullplant>> GetFullplantById(int id)
        {
            Plant? plant = await _context.Plants.FindAsync(id);
            Detail? detail = await _context.Details.FindAsync(id);

            if (plant == null || detail == null)
            {
                return NotFound();
            }

            List<string> images = new List<string>();
            BlobContainerClient blobContainerClient = _blobServiceClient.GetBlobContainerClient("images");

            await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync(prefix: $"{plant.Imagepath}"))
            {
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                Uri uri = blobClient.Uri;
                images.Add(uri.ToString());
            }

            return new Fullplant()
            {
                ID = id,
                Type = plant.Type,
                Namel = plant.Namel,
                Nameh = plant.Nameh,
                Imagepath = images,
                Description = detail.Description,
                Usage = detail.Usage,
                Pathogens = detail.Pathogens,
                Propagation = detail.Propagation
            };

        }

        [HttpGet("quiz")]
        public async Task<ActionResult<Quiz>> GetQuiz()
        {
            Quiz returnquiz = new Quiz();
            returnquiz.Names = new string[4];

            int plantCount = await _context.Plants.CountAsync();

            if (plantCount == 0)
            {
                return NotFound("Nincsenek növények az adatbázisban.");
            }

            Random rand = new Random();
            int index = 0;
            while (index < 4)
            {
                int id = rand.Next(1, plantCount);
                Plant plant = await _context.Plants.FindAsync(id);
                if (plant != null && !returnquiz.Names.Contains(plant.Namel))
                {
                    returnquiz.Names[index] = plant.Namel;
                    if (index == 0)
                    {
                        returnquiz.Imagepath = plant.Imagepath;
                    }
                    index++;
                }
            }

            return returnquiz;
        }

        [HttpPost("identify")]
        [Consumes("multipart/form-data")]
        public async Task<string> Identify([FromForm] IdentifyDTO data)
        {
            string language = "hu";
            if (data.Images == null || string.IsNullOrEmpty(data.Organs))
                return "Error: missing data";

            var stream = data.Images.OpenReadStream();
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(stream), "images", "image.jpeg");
            content.Add(new StringContent(data.Organs), "organs");

            string project = "all";
            if (plantnetApiKey == null)
            {
                return "Error: API key not found";
            }
            string URL = $"https://my-api.plantnet.org/v2/identify/{project}?api-key={plantnetApiKey}&lang={language}";

            var response = await _httpClient.PostAsync(URL, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        [HttpGet("search")]
        public async Task<Page<Plant>> Search([FromQuery] string? filter, [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 0)
        {
            if (pageSize <= 0)
            {
                pageSize = this.pageSize;
            }

            var query = _context.Plants.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(p => p.Namel.Contains(filter) || p.Nameh.Contains(filter));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.ID)
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Page<Plant> result = new Page<Plant>()
            {
                Items = items,
                PageNumber = pageNum,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return result;
        }

        [HttpGet("tips/latin/{latinName}")]
        public async Task<ActionResult<PlantTips>> GetTipsByLatinName(string latinName)
        {
            var existing = await _context.Translate
                .Where(t => t.LatinName == latinName)
                .Select(t => t.Tips)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                var tipsResult = await GetTipByName(latinName);

                if(tipsResult.Success) { return tipsResult.Data; }
                return NotFound(tipsResult.ErrorMessage);
            }
            else
            {
                return existing;
            }
        }

        [HttpGet("tips/hun/{hungarianName}")]
        public async Task<ActionResult<PlantTips>> GetTipsByHungarianName(string hungarianName)
        {
            var existing = await _context.PlantNames
                .Where(t => t.Language == "hu" && t.Name.Contains(hungarianName))
                .Select(t => t.Translate.Tips)
                .FirstOrDefaultAsync();

            if (existing == null)
            {
                //Translate
                var translation = await GetLatinName(hungarianName);

                if(translation.Success == false) { return StatusCode(500, translation.ErrorMessage); }

                string latinName = translation.Data!;

                var existingLatin = await _context.Translate
                    .Where(t => t.LatinName == latinName)
                    .Select(t => t.Tips)
                    .FirstOrDefaultAsync();

                if (existingLatin == null)
                {
                    var tipsResult = await GetTipByName(latinName);
                    if(tipsResult.Success)
                    {
                        if(string.IsNullOrEmpty(tipsResult.MetaData)) 
                        { 
                            return StatusCode(500,"Latin name not found"); 
                        } 
                        var saved = await SaveHungarian(hungarianName, latinName); 
                        if(!saved.Success) 
                        { 
                            return StatusCode(500, saved.ErrorMessage ?? "Unknown error"); 
                        }
                        return tipsResult.Data; 
                    } 
                    return NotFound(tipsResult.ErrorMessage);
                }
                else
                {
                    var saved = await SaveHungarian(hungarianName, latinName);
                    if (!saved.Success) 
                    { 
                        return StatusCode(500, saved.ErrorMessage ?? "Unknown error"); 
                    } 
                    return existingLatin;
                }
            }
            return existing;
        }

        //Add endpoint to picture recognition
        //Need recognition using PlantNet, use GetTipsFromLatinName

        private async Task<ServiceResult<string>> GetLatinName(string name)
        {
            string sparqlQuery = $@"
                        SELECT ?latin WHERE {{
                        ?item rdfs:label ""{name}""@hu.
                        ?item wdt:P225 ?latin.
                    }} LIMIT 1";

            string url = $"{sparqlEndpoint}?query={HttpUtility.UrlEncode(sparqlQuery)}&format=json";

            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            httpRequestMessage.Headers.Add("Accept", "application/sparql-results+json");
            httpRequestMessage.Headers.Add("User-Agent", "diszkerteszAPI/1.0");

            try
            {
                var response = await _httpClient.SendAsync(httpRequestMessage);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    //Might have some problems, need to be tested with actual data, but it should work
                    string latinName = (string)JObject.Parse(jsonResponse).SelectToken("$.results.bindings[0].latin.value");

                    if (!string.IsNullOrEmpty(latinName))
                    {
                        return ServiceResult<string>.SuccessResult(latinName);
                    }
                    else
                    {
                        return ServiceResult<string>.FailureResult("Latin name not found");
                    }
                }
                else
                {
                    return ServiceResult<string>.FailureResult($"SPARQL query error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.FailureResult($"SPARQL query exception: {ex.Message}");
            }
        }

        private async Task<ServiceResult<PerenualSearchDTO>> GetPerenualSpecies(string name)
        {
            if (perApiKey == null) { return ServiceResult<PerenualSearchDTO>.FailureResult("API key not found"); }

            var searchResponse = await _httpClient.GetFromJsonAsync<PerenualSearchDTO>($"https://perenual.com/api/v2/species-list?key={perApiKey}&q={name}");

            if (searchResponse?.Data == null || searchResponse.Data.Count <= 0)
            {
                return ServiceResult<PerenualSearchDTO>.FailureResult("Plant not found");
            }

            return ServiceResult<PerenualSearchDTO>.SuccessResult(searchResponse);
        }

        private async Task<ServiceResult<PerenualDetailDTO>> GetPerenualDetails(int id)
        {
            if (perApiKey == null) { return ServiceResult<PerenualDetailDTO>.FailureResult("API key not found"); }
            await Task.Delay(1000);
            var detailsResponse = await _httpClient.GetFromJsonAsync<PerenualDetailDTO>($"https://perenual.com/api/v2/species/details/{id}?key={perApiKey}");

            if (detailsResponse == null)
            {
                return ServiceResult<PerenualDetailDTO>.FailureResult("API error (ID not found)");
            }

            return ServiceResult<PerenualDetailDTO>.SuccessResult(detailsResponse);
        }

        private async Task<ServiceResult<PlantTips>> GetTipByName(string name)
        {
            var searchResult = await GetPerenualSpecies(name);

            if (!searchResult.Success) { return ServiceResult<PlantTips>.FailureResult("Search went wrong"); }

            int plantId = searchResult.Data.Data.First().Id;

            var detailsResult = await GetPerenualDetails(plantId);

            if (!detailsResult.Success) { return ServiceResult<PlantTips>.FailureResult("Details went wrong"); }

            PerenualDetailDTO detailsData = detailsResult.Data;

            var tips = new PlantTips()
            {
                Water = detailsData.Watering,
                Light = detailsData.Sunlight[0],
                Cycle = detailsData.Cycle,
                CareLevel = detailsData.Care_level,
            };

            if (detailsData.Poisonous_to_pets || detailsData.Poisonous_to_humans) tips.Poisonous = "Igen";
            else tips.Poisonous = "Nem";

            if (detailsData.Soil.Count <= 0)
            {
                tips.Soil = "Nem releváns";
            }
            else
            {
                tips.Soil = detailsData.Soil[0];
            }

            string latinName = searchResult.Data.Data.First().Scientific_Name[0];
            var translation = new Translate()
            {
                PerenualID = plantId,
                //Not sure if this is the best way to do this, but it works for now
                LatinName = latinName,
                Tips = tips
            };

            foreach (string otherName in searchResult.Data.Data[0].Other_name)
            {
                translation.Names.Add(new PlantName()
                {
                    Name = otherName,
                    Language = "en",
                });
            }

            try
            {
                _context.Translate.Add(translation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return ServiceResult<PlantTips>.FailureResult("Database error");
            }

            return ServiceResult<PlantTips>.SuccessResult(tips, latinName);
        }

        private async Task<ServiceResult> SaveHungarian(string hungarianName, string englishName)
        {
            try
            {
                var existingTranslation = await _context.PlantNames
                    .Where(t => t.Language == "en" && t.Name.Contains(englishName))
                    .Select(t => t.Translate)
                    .FirstOrDefaultAsync();

                //should exist if GetTipByName was successful, but just in case
                if (existingTranslation == null) { return ServiceResult.FailureResult("Translation not found"); }
                
                existingTranslation.Names.Add(new PlantName()
                {
                    Name = hungarianName,
                    Language = "hu",
                });

                await _context.SaveChangesAsync();
                //Update existing entry with new Hungarian name                            
            }
            catch (DbUpdateException ex)
            {
                return ServiceResult.FailureResult("Database error");
            }
             return ServiceResult.SuccessResult("Success");
        }
    }
}
