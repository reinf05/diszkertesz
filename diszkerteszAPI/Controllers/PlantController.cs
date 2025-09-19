using diszkerteszAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace diszkerteszAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlantController : ControllerBase
    {
        private readonly diszkerteszDbContext _context;
        public PlantController(diszkerteszDbContext context)
        {
            _context = context;
        }

        [HttpGet("plants")]
        public async Task<IEnumerable<Plant>> GetPlants()
        {
            return await _context.Plants.OrderBy(p => p.ID).ToListAsync();

        }

        [HttpGet("plants/{id}")]
        public async Task<ActionResult<Plant>> GetPlantById(int id)
        {
            return await _context.Plants.FindAsync(id);
        }

        [HttpGet("details")]
        public async Task<IEnumerable<Detail>> GetDetails()
        {
            return await _context.Details.OrderBy(d => d.Plant_ID).ToListAsync();
        }

        [HttpGet("details/{id}")]
        public async Task<ActionResult<Detail>> GetDetailById(int id)
        {
            return await _context.Details.FindAsync(id);
        }

        [HttpGet("fullplants")]
        public async Task<IEnumerable<object>> GetFullPlants(){
            List<Plant> plantlist = (List<Plant>)await GetPlants();
            List<Detail> detaillist = (List<Detail>)await GetDetails();

            List<Fullplant> returnlist = new List<Fullplant>();

            int count = 0;
            while (count < plantlist.Count)
            {
                int id = plantlist[count].ID;
                returnlist.Add(new Fullplant());
                returnlist[count].ID = id;
                returnlist[count].Type = plantlist[count].Type;
                returnlist[count].Namel = plantlist[count].Namel;
                returnlist[count].Nameh = plantlist[count].Nameh;
                returnlist[count].Imagepath = plantlist[count].Imagepath;
                returnlist[count].Description = detaillist[count].Description;
                returnlist[count].Usage = detaillist[count].Usage;
                returnlist[count].Pathogens = detaillist[count].Pathogens;
                returnlist[count].Propagation = detaillist[count].Propagation;

                count++;
            }

            return returnlist;
        }

        [HttpGet("fullplants/{id}")]
        public async Task<ActionResult<Fullplant>> GetFullplantById(int id)
        {
            Plant plant = await _context.Plants.FindAsync(id);
            Detail detail = await _context.Details.FindAsync(id);

            return new Fullplant()
            {
                ID = id,
                Type = plant.Type,
                Namel = plant.Namel,
                Nameh = plant.Nameh,
                Imagepath = plant.Imagepath,
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

    }
}
