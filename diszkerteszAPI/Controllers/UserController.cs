using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using diszkerteszAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;

namespace diszkerteszAPI.Controllers
{
    [Authorize]
    [RequiredScope("user_access")]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly diszkerteszDbContext _context;
        private readonly IHttpContextAccessor _httpContext;
        private readonly BlobServiceClient _blobServiceClient;

        public UserController(diszkerteszDbContext context, IHttpContextAccessor httpContextAccessor, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _httpContext = httpContextAccessor;
            _blobServiceClient = blobServiceClient;
        }

        [HttpPost("create-user")]
        public async Task<User> CreateUserAsync()
        {
            var meeid = (_httpContext.HttpContext?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")) ?? _httpContext.HttpContext?.User.FindFirst("oid") ?? throw new Exception("Meeid not found in token.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Meeid == meeid.Value);

            if (user == null)
            {
                user = new User
                {
                    Meeid = meeid.Value,
                    Provisioned = false
                };

                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.Entry(user).State = EntityState.Detached;
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Meeid == meeid.Value);
                }

            }
            if (!user!.Provisioned)
            {
                await ProvisionUserAsync(user);
            }
            return user;
        }

        private async Task ProvisionUserAsync(User user)
        {
            string containerName = $"user-{user.Id.ToString()}";
            var blobContainerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
            blobContainerClient.Value.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);


            // Provisioning logic here
            user.Provisioned = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        private async Task<User?> GetCurrentUserAsync(string meeid)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Meeid == meeid);
        }

        [HttpGet("user-list")]
        public async Task<IActionResult> GetUserListAsync()
        {
            try
            {
                var meeid = (_httpContext.HttpContext?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")) ?? _httpContext.HttpContext?.User.FindFirst("oid") ?? throw new Exception("Meeid not found in token.");

                User? user = await GetCurrentUserAsync(meeid.Value);

                if (user == null)
                {
                    return NotFound($"User not found with id {meeid.Value}.");
                }

                var usersSharedList = await _context.UsersShared
                    .Where(u => u.Owner == user.Id)
                    .ToListAsync();

                return Ok(usersSharedList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("upload-image")]
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var meeid = (_httpContext.HttpContext?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")) ?? _httpContext.HttpContext?.User.FindFirst("oid") ?? throw new Exception("Meeid not found in token.");

            User user = await GetCurrentUserAsync(meeid.Value);

            string containerName = $"user-{user.Id.ToString()}";
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(file.FileName);


            using (var stream = file.OpenReadStream())
            {
                var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = file.ContentType
                };

                await blobClient.UploadAsync(stream, overwrite: true);
                await blobClient.SetHttpHeadersAsync(blobHttpHeaders);
            }
            return blobClient.Uri.ToString();
        }

        [HttpPost("post-list")]
        public async Task<IActionResult> PostUserListAsync([FromBody] UsersShared usersSharedPost)
        {
            var meeid = (_httpContext.HttpContext?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")) ?? _httpContext.HttpContext?.User.FindFirst("oid") ?? throw new Exception("Meeid not found in token.");
            User user = await GetCurrentUserAsync(meeid.Value);

            usersSharedPost.Owner = user.Id;

            try
            {
                _context.UsersShared.Add(usersSharedPost);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database update error: {ex.Message}");
            }

            return new OkResult();
        }

        [HttpDelete("delete-item/{id}")]
        public async Task<IActionResult> DeleteUserItemAsync(int id)
        {
            try
            {
                await _context.UsersShared
                    .Where(u => u.Id == id)
                    .ExecuteDeleteAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database update error: {ex.Message}");
            }

            return new OkResult();
        }

        [HttpGet("get-item/{id}")]
        public async Task<IActionResult> GetUserItemAsync(int id)
        {
            try
            {
                var item = await _context.UsersShared
                    .FirstOrDefaultAsync(u => u.Id == id);
                if (item == null)
                {
                    return NotFound($"Item with id {id} not found.");
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("edit-item")]
        public async Task<IActionResult> EditUserItemAsync([FromBody] UsersShared item)
        {
            try
            {
                await _context.UsersShared
                    .Where(u => u.Id == item.Id)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(p => p.Name, item.Name)
                        .SetProperty(p => p.Description, item.Description)
                        .SetProperty(p => p.Pictureurl, item.Pictureurl)
                        .SetProperty(p => p.PerenualID, item.PerenualID)
                        .SetProperty(p => p.LatinName, item.LatinName)
                        .SetProperty(p => p.Tips, item.Tips));

            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Database update error: {ex.Message}");
            }
            return new OkResult();
        }

        [HttpPost("delete-image")]
        public async Task<IActionResult> DeleteImageAsync([FromQuery] string fileName)
        {
            var meeid = (_httpContext.HttpContext?.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")) ?? _httpContext.HttpContext?.User.FindFirst("oid") ?? throw new Exception("Meeid not found in token.");
            User user = await GetCurrentUserAsync(meeid.Value);
            string containerName = $"user-{user.Id.ToString()}";
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = blobContainerClient.GetBlobClient(fileName);
            try
            {
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting image: {ex.Message}");
            }
            return new OkResult();
        }
    }
}
