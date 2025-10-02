using Taskist.Core.Common;

namespace Taskist.Web.Helpers.Common;

public static class WebHelper
{
    public static string UploadedFile(IFormFile formFile, string rootPath,
        FileUploadLocationEnum location = FileUploadLocationEnum.General)
    {
        string uniqueFileName = null;

        if (formFile != null)
        {
            string uploadsFolder = Path.Combine(rootPath, "images", location.ToString());
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            uniqueFileName = Guid.NewGuid().ToString() + "_" + formFile.FileName.Replace(" ", "");
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                formFile.CopyTo(fileStream);
            }
        }
        return uniqueFileName;
    }

    public static void DeleteFile(string rootPath, string fileName,
        FileUploadLocationEnum location = FileUploadLocationEnum.General)
    {
        string uploadsFolder = Path.Combine(rootPath, "images", location.ToString());
        string filePath = Path.Combine(uploadsFolder, fileName);

        if (File.Exists(filePath))
            File.Delete(fileName);
    }
}
