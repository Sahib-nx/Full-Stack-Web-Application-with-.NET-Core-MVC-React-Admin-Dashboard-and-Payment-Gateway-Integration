using System;

namespace CRM.Interfaces;

public interface ICloudinaryService
{
      public Task<string> UploadImageAsync(IFormFile file);

}
