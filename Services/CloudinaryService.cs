using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using EXE02_Backend_RE_CAFE.Exceptions;
using EXE02_Backend_RE_CAFE.Interfaces;
using EXE02_Backend_RE_CAFE.Models;

namespace EXE02_Backend_RE_CAFE.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary? _cloudinary;
        private readonly bool _isConfigured;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinaryOptions)
        {
            var settings = cloudinaryOptions.Value;
            _isConfigured = !string.IsNullOrWhiteSpace(settings.CloudName) &&
                            !string.IsNullOrWhiteSpace(settings.ApiKey) &&
                            !string.IsNullOrWhiteSpace(settings.ApiSecret);

            if (_isConfigured)
            {
                _cloudinary = new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret))
                {
                    Api = { Secure = true }
                };
            }
        }

        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file, string folder)
        {
            if (!_isConfigured)
            {
                throw new BadRequestException("Cloudinary is not configured. Please set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY and CLOUDINARY_API_SECRET.");
            }

            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("File is required.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Only image files are allowed.");
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary!.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new BadRequestException($"Cloudinary upload failed: {result.Error.Message}");
            }

            if (result.SecureUrl == null || string.IsNullOrWhiteSpace(result.PublicId))
            {
                throw new BadRequestException("Cloudinary upload failed to return valid URL.");
            }

            return (result.SecureUrl.ToString(), result.PublicId);
        }

        public async Task<(string Url, string PublicId)> UploadRawFileAsync(IFormFile file, string folder)
        {
            if (!_isConfigured)
            {
                throw new BadRequestException("Cloudinary is not configured. Please set CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY and CLOUDINARY_API_SECRET.");
            }

            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("File is required.");
            }

            await using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var result = await _cloudinary!.UploadAsync(uploadParams);

            if (result.Error != null)
            {
                throw new BadRequestException($"Cloudinary raw upload failed: {result.Error.Message}");
            }

            if (result.SecureUrl == null || string.IsNullOrWhiteSpace(result.PublicId))
            {
                throw new BadRequestException("Cloudinary raw upload failed to return valid URL.");
            }

            return (result.SecureUrl.ToString(), result.PublicId);
        }
    }
}
