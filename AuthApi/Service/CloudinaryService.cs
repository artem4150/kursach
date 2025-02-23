using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Npgsql.BackendMessages;
using System.Security.Principal;

public class CloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(string cloudName, string apiKey, string apiSecret)
    {
        Account account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Файл не выбран или пустой.");

        using (var stream = file.OpenReadStream())
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                // Дополнительные параметры (например, трансформации) можно указать здесь
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult;
        }
    }
}
