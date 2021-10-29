using DAL;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using System.IO;
using HttpMultipartParser;
using Azure.Storage.Blobs.Models;

namespace Service
{
    public interface IHouseService
    {
        public Task AddHouse(House house);
        public Task RemoveHouse(Guid id);
        public Task<List<House>> GetAllHouses();
        public Task<Image> AddImageToHouse(Guid id, FilePart file);
    }
    public class HouseService : IHouseService
    {
        private readonly CloudDBContext _context;
        private readonly ILogger _logger;
        private readonly BlobContainerClient _containerClient;

        public HouseService(CloudDBContext context, ILogger<HouseService> logger)
        {
            _context = context;
            _logger = logger;

            // Connection string for blobstorage
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            // Container client where the house images are stored
            _containerClient = new BlobContainerClient(connectionString, "house-images");
            // Make sure the container exists and is publicly accessible
            _containerClient.CreateIfNotExists(PublicAccessType.BlobContainer);            
        }

        // Add a new house
        public async Task AddHouse(House house)
        {
            await _context.Houses.AddAsync(house);
            await _context.SaveChangesAsync();
        }

        public async Task<Image> AddImageToHouse(Guid id, FilePart file)
        {
            // Find the house the image needs to be added to
            House house = await _context.Houses.FindAsync(id);
            
            // Create a new image
            Image image = new Image()
            {
                Name = file.FileName,
            };

            // Add the image to the house
            house.Images.Add(image);
            
            // Save changes so the image gets a Guid
            await _context.SaveChangesAsync();
            
            // Create name for blob
            string fileType = file.ContentType.Replace("image/", ".");
            string blobName = image.Id + fileType;
            
            // Save image to blob
            await _containerClient.UploadBlobAsync(blobName, file.Data);

            // Add blob Uri to image object
            var blobClient = _containerClient.GetBlobClient(blobName);
            image.Url = blobClient.Uri.OriginalString;
            await _context.SaveChangesAsync();

            // Return the saved image
            return image;
        }

        public async Task<List<House>> GetAllHouses()
        {
            return await _context.Houses.ToListAsync();
        }

        public async Task RemoveHouse(Guid id)
        {
            var house = await _context.Houses.FindAsync(id);
            _context.Houses.Remove(house);
            await _context.SaveChangesAsync();
        }
    }
}
