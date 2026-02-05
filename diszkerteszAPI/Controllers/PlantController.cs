using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using diszkerteszAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;
using System.Drawing;
using System.IO;
using System.Text;
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
        public PlantController(diszkerteszDbContext context, BlobServiceClient BlobServiceClient)
        {
            _blobServiceClient = BlobServiceClient;
            _context = context;
            _httpClient = new HttpClient();
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

        //[HttpPost("identify")]
        //public async Task<string> Identify([FromForm] IFormFile images, [FromForm] string organs)
        //{
        //    string language = "hu";
        //    if (images == null || string.IsNullOrEmpty(organs))
        //        return "Error: missing data";

        //    var stream = images.OpenReadStream();
        //    var content = new MultipartFormDataContent();
        //    content.Add(new StreamContent(stream), "images", "image.jpeg");
        //    content.Add(new StringContent(organs), "organs");

        //    string project = "all";
        //    if (plantnetApiKey == null)
        //    {
        //        return "Error: API key not found";
        //    }
        //    string URL = $"https://my-api.plantnet.org/v2/identify/{project}?api-key={apiKey}&lang={language}";

        //    var response = await _httpClient.PostAsync(URL, content);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsonResponse = await response.Content.ReadAsStringAsync();
        //        return jsonResponse;
        //    }
        //    else
        //    {
        //        return $"Error: {response.StatusCode}";
        //    }
        //}

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

                if (perApiKey == null) { return StatusCode(500, "Error: API key not found"); }

                var searchResponse = await _httpClient.GetFromJsonAsync<PerenualSearchDTO>($"https://perenual.com/api/v2/species-list?key={perApiKey}&q={latinName}");

                if (searchResponse?.Data == null || !searchResponse.Data.Any())
                {
                    return NotFound("No plant found with the given Latin name.");
                }

                int plantId = searchResponse.Data.First().Id;
                var detailsResponse = await _httpClient.GetFromJsonAsync<PerenualDetailDTO>($"https://perenual.com/api/v2/species/details/{plantId}?key={perApiKey}");

                if (detailsResponse is null)
                {
                    return NotFound("No details found for the given plant ID.");
                }

                var tips = new PlantTips()
                {
                    Water = detailsResponse.Watering,
                    Light = detailsResponse.Sunlight[0],
                    Cycle = detailsResponse.Cycle,
                    CareLevel = detailsResponse.Care_level,
                };

                if (detailsResponse.Poisonous_to_pets || detailsResponse.Poisonous_to_humans) tips.Poisonous = "Igen";
                else tips.Poisonous = "Nem";

                if (detailsResponse.Soil.Count <= 0)
                {
                    tips.Soil = "Nem releváns";
                } else
                {
                    tips.Soil = detailsResponse.Soil[0];
                }

                var translation = new Translate()
                {
                    PerenualID = plantId,
                    LatinName = latinName,
                    Tips = tips
                };

                foreach(string name in searchResponse.Data[0].Other_name)
                {
                    translation.EnglishNames ??= new List<string>();
                    translation.EnglishNames.Add(name);
                }

                _context.Translate.Add(translation);
                await _context.SaveChangesAsync();

                return tips;
            }
            else
            {
                return existing;
            }
        }

        [HttpGet("tips/hun/{hungarianName}")]
        public async Task<ActionResult<PlantTips>> GetTipsByHungarianName(string hungarianName)
        {
            var existing = await _context.Translate
                .Where(t => t.HungarianNames.Contains(hungarianName))
                .Select(t => t.Tips)
                .FirstOrDefaultAsync();

            if(existing == null)
            {
                //Translate
                //Check db if English name is already there
                    //If yes, add Hungarian name and return the given tips
                    //If no, get ID from /species-list and then the details
                        //Refactor by giving this its own function
                        //Save to db
                        //Refactor by giving the save its own function
                        //Return the given tips

            }
            else
            {
                return existing;
            }

                return NotFound("NIGGER");
        }

        //Add endpoint to picture recognition
        //Need recognition using PlantNet, use GetTipsFromLatinName
    }
}
