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
        public PlantController(diszkerteszDbContext context, BlobServiceClient BlobServiceClient)
        {
            _blobServiceClient = BlobServiceClient;
            _context = context;
        }


        [HttpGet("plants/{pageNum}")]
        public async Task<Page<Plant>> GetPlantByPage(int pageNum = 1)
        {
            if(pageNum < 1)
            {
                pageNum = 1;
            }
            int totalCount = await _context.Plants.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if(pageNum > totalPages) pageNum = totalPages;

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
            while(index < 4)
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
        //    string apiKey = Environment.GetEnvironmentVariable("API-KEY");
        //    if (apiKey == null)
        //    {
        //        return "Error: API key not found";
        //    }
        //    string URL = $"https://my-api.plantnet.org/v2/identify/{project}?api-key={apiKey}&lang={language}";

        //    HttpClient client = new HttpClient();

        //    var response = await client.PostAsync(URL, content);

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
            if(pageSize <= 0)
            {
                pageSize = this.pageSize;
            }

            var query = _context.Plants.AsQueryable();

            if(!string.IsNullOrEmpty(filter))
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

    }
}
