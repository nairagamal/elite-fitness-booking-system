namespace elite.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile, string folderName);
        void DeleteImage(string imagePath);
        bool ImageExists(string imagePath);
    }
}
