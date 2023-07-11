namespace CripexLibrary.Services.FileUploadService
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, bool isAdminController);
        //Task<string> UploadProfilePhotoAsync(IFormFile file);
    }
}
