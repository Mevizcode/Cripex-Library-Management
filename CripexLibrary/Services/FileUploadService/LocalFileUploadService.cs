namespace CripexLibrary.Services.FileUploadService
{
    public class LocalFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment environment;
        public LocalFileUploadService(IWebHostEnvironment environment)
        {
            this.environment = environment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, bool isAdminController = false)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName);
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Invalid file type. Please upload a JPG, PNG, or GIF image.");
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
            //generate a unique filename
            var fileName = $"{fileNameWithoutExtension}{Guid.NewGuid().ToString()}{Path.GetExtension(file.FileName)}";

            var directoryName = isAdminController ? "user-profile-photo" : "book-photos";


            var filePath = Path.Combine(environment.ContentRootPath, "wwwroot", "images", directoryName, fileName);
            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);
            return fileName;
        }
    }
}
