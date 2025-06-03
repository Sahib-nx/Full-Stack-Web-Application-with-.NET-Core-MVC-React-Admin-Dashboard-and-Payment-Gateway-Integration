using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using dotenv.net;

namespace CRM.Services;

public class CloudinaryService
{

    private readonly Cloudinary _cloudinary;

    public CloudinaryService()
    {
        try
        {
            DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
        }
        catch (Exception Error)
        {

            throw new InvalidOperationException("Failed to load DotEnv file", Error);
        }
        var CloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");

        if (string.IsNullOrEmpty(CloudinaryUrl))
        {

            throw new InvalidOperationException("CLOUDINARY_URL is not set");
        }

        _cloudinary = new Cloudinary(CloudinaryUrl) { Api = { Secure = true } };
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {

        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("File is empty");
        }

        using var stream = file.OpenReadStream();

        var uploadparams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true
        };
        var UploadResult = await _cloudinary.UploadAsync(uploadparams);
        if (UploadResult.Error != null)
        {
            throw new InvalidOperationException($"Failed to upload image: {UploadResult.Error.Message}");
        }

        return UploadResult.SecureUrl.ToString();
    }




}
