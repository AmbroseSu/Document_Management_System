using System.Diagnostics;

using System.Globalization;
using BusinessObject.Option;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using DataAccess.DTO.Request;
using DocumentFormat.OpenXml.Packaging;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using Repository;
using Repository.Impl;
using Color = SixLabors.ImageSharp.Color;
using DocumentVersion = BusinessObject.DocumentVersion;
using Font = SixLabors.Fonts.Font;
using FontStyle = SixLabors.Fonts.FontStyle;
using HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment;
using Image = SixLabors.ImageSharp.Image;
using PdfDocument = Syncfusion.Pdf.PdfDocument;
using Point = SixLabors.ImageSharp.Point;
using PointF = SixLabors.ImageSharp.PointF;
using TextAlignment = iText.Layout.Properties.TextAlignment;
using VerticalAlignment = SixLabors.Fonts.VerticalAlignment;

namespace Service.Impl;

public class FileService : IFileService
{
    private readonly string _storagePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "storage");
    private readonly string _host;
    private readonly IUnitOfWork _unitOfWork;

    public FileService(IOptions<AppsetingOptions> options, IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _host = options.Value.Host;
    }


    public async Task<string> SaveUploadFile(IFormFile file)
    {
        var filePath = Path.Combine(_storagePath, "document", "UploadedFiles", file.FileName);
        Directory.CreateDirectory(_storagePath);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task<string> SaveAvatar(IFormFile file, string id)
    {
        var avatarFolder = Path.Combine(_storagePath, "avatar");
        Directory.CreateDirectory(avatarFolder);

        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{id}{fileExt}"; // tạo tên mới
        var filePath = Path.Combine(avatarFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public string CreateAVersionFromUpload(string fileName, Guid versionId, Guid documentId, string versionName,DateTime? validFroms)
    {
        var path = Path.Combine(_storagePath, "document", "UploadedFiles", fileName);
        var targetDirectory = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString());

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        
        // Tạo đường dẫn đích
        var targetPath = Path.Combine(targetDirectory, versionName + ".pdf");

        // Di chuyển file
        File.Move(path, targetPath);
        Thread.Sleep(1000);
        try
        {
            // Đường dẫn tệp cá nhân hóa, ví dụ: "C:\Users\YourName\Documents\myfile.yourname"
            string fileExtension = ".conf-dms"; // Phần mở rộng tùy chỉnh
            string filePath = Path.Combine(targetDirectory, $"config{fileExtension}");

            // Đảm bảo thư mục tồn tại
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Chuyển DateTime thành chuỗi với định dạng cụ thể
           string content = validFroms.HasValue ? validFroms.Value.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Ghi nội dung vào tệp
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            throw new Exception("Lỗi khi tạo tệp.", ex);
        }
        var url = _host + "/api/Document/view-file/" + documentId + "?version=0&isArchive=false";
        // Trả về đường dẫn mới của file (hoặc bạn có thể trả về tên file tùy mục đích)
        return url;
    }

    public string ArchiveDocument(string fileName, Guid documentId, Guid versionId, Guid archiveId)
    {
        fileName += ".pdf";
        var path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), fileName);

        var targetDirectory = Path.Combine(_storagePath, "archive_document", archiveId.ToString());

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        var targetPath = Path.Combine(targetDirectory, fileName);
        File.Copy(path, targetPath);
        var configPath = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), "config.conf-dms");
        if (File.Exists(configPath))
            File.Copy(configPath, Path.Combine(targetDirectory, "config.conf-dms"));
        var url = _host + "/api/Document/view-file/" + archiveId + "?version=1&isArchive=true";
        return url;
    }

    public string? GetFileSize(Guid documentId, Guid versionId, string fileName)
    {
        var tmp = fileName + ".pdf";
        var path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), tmp);
        var info = new FileInfo(path);
        if (!info.Exists)
        {
            tmp = fileName + ".docx";
            path = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString(), tmp);
            info = new FileInfo(path);
            if (!info.Exists)
            {
                tmp = fileName + ".pdf";
                path = Path.Combine(_storagePath, "archive_document", documentId.ToString(), tmp);
                info = new FileInfo(path);
                if(!info.Exists) return null;
            }
        }
        
        var size = info.Length;
        var sizeInKB = Math.Round(size / 1024.0, 2); // Chuyển đổi sang KB
        var sizeInMB = Math.Round(sizeInKB / 1024.0, 2); // Chuyển đổi sang MB
        var sizeInGB = Math.Round(sizeInMB / 1024.0, 2); // Chuyển đổi sang GB
        if (sizeInMB > 1024)
            return sizeInGB.ToString(CultureInfo.InvariantCulture) + " GB";
        if (sizeInKB > 1024)
            return sizeInMB.ToString(CultureInfo.InvariantCulture) + " Mb";
        return sizeInKB.ToString(CultureInfo.InvariantCulture) + " KB";
    }

    public async Task<string> SaveSignature(IFormFile file, string userId)
    {
        var avatarFolder = Path.Combine(_storagePath, "signature");
        Directory.CreateDirectory(avatarFolder);

        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{userId}{fileExt}"; // tạo tên mới
        var filePath = Path.Combine(avatarFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    public async Task<IActionResult> GetSignature(string userId)
    {
        var filePath = Path.Combine(_storagePath, "signature", userId);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }

        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);

        return new FileContentResult(bytes, contentType);
    }

    // public async Task<IActionResult> GetPdfFile(Guid id)
    // {
    //     var filePath = Path.Combine(_storagePath, "document", "UploadedFiles", id+".pdf");;
    //
    //     if (!File.Exists(filePath))
    //     {
    //         throw new FileNotFoundException("File not found", filePath);
    //     }
    //
    //     const string contentType = "application/pdf";
    //     var bytes = await File.ReadAllBytesAsync(filePath);
    //
    //     return new FileContentResult(bytes, contentType);
    // }

    public async Task<IActionResult> GetPdfFile(string filePath,bool isLegal = false)
    {
        var path = Path.Combine(_storagePath, filePath);
        var extension = Path.GetExtension(path);
        var tmp = extension;
        var footerPath = Path.Combine(_storagePath, "tmp", Path.GetFileNameWithoutExtension(path) + "_footer.pdf");
        switch (extension)
        {
            case ".pdf":
                extension = "pdf";
                if (!isLegal)
                    AddFooterToPdf(path, footerPath);
                break;
            case ".docx":
                extension = "openxmlformats-officedocument.wordprocessingml.document";
                break;
            case ".doc":
                extension = "vnd.ms-word";
                break;
        }
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }
        
        var contentType = $"application/{extension}";
        
        var bytes = await File.ReadAllBytesAsync(isLegal ? path : footerPath);
        if (extension == "pdf" &&  (!isLegal))
        {
            // Xóa file tạm sau khi đọc
            File.Delete(footerPath);
        }

        return new FileContentResult(bytes, contentType)
            {
                FileDownloadName = Guid.NewGuid() + tmp
            }
            ;
    }
    public async Task<IActionResult> GetTemplateFile(string filePath)
    {
        var path = Path.Combine(_storagePath, filePath);
        var extension = Path.GetExtension(path);
        var tmp = extension;
        switch (extension)
        {
            case ".pdf":
                extension = "pdf";
          
                break;
            case ".docx":
                extension = "openxmlformats-officedocument.wordprocessingml.document";
                break;
            case ".doc":
                extension = "vnd.ms-word";
                break;
        }
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("File not found", path);
        }
        
        var contentType = $"application/{extension}";
        
        var bytes = await File.ReadAllBytesAsync(path);
        
        return new FileContentResult(bytes, contentType)
            {
                FileDownloadName = Guid.NewGuid() + tmp
            }
            ;
    }
    public void AddFooterToPdf(string inputFilePath, string outputFilePath)
    {
        if (!File.Exists(inputFilePath))
        {
            Console.WriteLine("Input file not found.");
            return;
        }

        using (var pdfReader = new PdfReader(inputFilePath))
        using (var pdfWriter = new PdfWriter(outputFilePath))
        using (var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader, pdfWriter))
        {
            var document = new iText.Layout.Document(pdfDocument);

            // Define the footer text
            var footerText = new Paragraph("This is a copy without legal value, created by the DMS system")
                .SetFontSize(6)
                .SetFontColor(new DeviceRgb(0xb4, 0xb6, 0xb8))
                .SetTextAlignment(TextAlignment.CENTER);

            // Add the footer to each page
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var pageSize = pdfDocument.GetPage(i).GetPageSize();
                float x = pageSize.GetWidth() / 2;
                float y = pageSize.GetBottom() + 20; // Adjust the Y position as needed
                document.ShowTextAligned(footerText, x, y, i, TextAlignment.CENTER, iText.Layout.Properties.VerticalAlignment.BOTTOM, 0);
            }
        }

        Console.WriteLine($"Footer added. Modified file saved at: {outputFilePath}");
    }
    
    public async Task<(byte[] FileBytes, string FileName, string ContentType)> GetFileBytes(string filePath)
    {
        var path = Path.Combine(_storagePath, filePath);

        if (!File.Exists(path))
            throw new FileNotFoundException("File not found", path);

        var extension = Path.GetExtension(path).ToLower();
        string contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".docx" => "application/msword",
            _ => "application/octet-stream"
        };

        var bytes = await File.ReadAllBytesAsync(path);
        var fileName = Path.GetFileName(path);

        return (bytes, fileName, contentType);
    }

    public async Task<IActionResult> GetAvatar(string fileName)
    {
        var filePath = Path.Combine(_storagePath, "avatar", fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        
        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);
        
        return new FileContentResult(bytes, contentType);
    }

    public async Task<IActionResult> ConvertDocToPdf(IFormFile file)
    {
        // Ensure the file is a .doc or .docx
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (fileExtension != ".doc" && fileExtension != ".docx")
        {
            throw new ArgumentException("Invalid file format. Only .doc and .docx are supported.");
        }
    
        // Convert Word to PDF directly from the file stream
        await using var inputStream = file.OpenReadStream();
        using var wordDocument = new WordDocument(inputStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        return new FileContentResult(pdfStream.ToArray(), "application/pdf")
            ;
    }
    
    private async Task<string> ConvertDocToPdfPrivate(IFormFile file)
    {
        // Ensure the file is a .doc or .docx
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (fileExtension != ".doc" && fileExtension != ".docx")
        {
            throw new ArgumentException("Invalid file format. Only .doc and .docx are supported.");
        }
    
        // Convert Word to PDF directly from the file stream
        await using var inputStream = file.OpenReadStream();
        using var wordDocument = new WordDocument(inputStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        
        var pdfFileName = Guid.NewGuid() + ".pdf";
        var pdfFilePath = Path.Combine(_storagePath, "tmp", pdfFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(pdfFilePath));
        await File.WriteAllBytesAsync(pdfFilePath, pdfStream.ToArray());

        return pdfFilePath;
        // return new FileContentResult(pdfStream.ToArray(), "application/pdf")
        //     ;
    }
    
    public async Task<IActionResult> ConvertDocToPdf(string path)
    {
        
        // Ensure the file is a .doc or .docx
        var fileExtension = Path.GetExtension(path).ToLower();
        if (fileExtension != ".doc" && fileExtension != ".docx")
        {
            throw new ArgumentException("Invalid file format. Only .doc and .docx are supported.");
        }
    
        // Convert Word to PDF directly from the file stream
        await using var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var wordDocument = new WordDocument(inputStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        return new FileContentResult(pdfStream.ToArray(), "application/pdf")
            {
                FileDownloadName = Guid.NewGuid() + ".pdf"
            }
            ;
    }
    
    
    public async Task<string> ConvertDocToPdfPhysic(string path)
    {
        
        // Ensure the file is a .doc or .docx
        var fileExtension = Path.GetExtension(path).ToLower();
        if (fileExtension != ".doc" && fileExtension != ".docx")
        {
            throw new ArgumentException("Invalid file format. Only .doc and .docx are supported.");
        }
    
        // Convert Word to PDF directly from the file stream
        await using var inputStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var wordDocument = new WordDocument(inputStream, FormatType.Automatic);
        using var renderer = new DocIORenderer();
        var pdfDocument = renderer.ConvertToPDF(wordDocument);
        using var pdfStream = new MemoryStream();
        pdfDocument.Save(pdfStream);
        pdfStream.Position = 0;
        await File.WriteAllBytesAsync(Path.Combine(_storagePath,"tmp",Path.GetFileNameWithoutExtension(path)+".pdf"),pdfStream.ToArray());
        return Path.Combine(_storagePath,"tmp",Path.GetFileNameWithoutExtension(path)+".pdf");
    }
    public void ConvertDocToDocx(string inputPath, string outputDir)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "libreoffice",
                Arguments = $"--headless --convert-to docx --outdir \"{outputDir}\" \"{inputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string stdout = process.StandardOutput.ReadToEnd();
        string stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"LibreOffice failed: {stderr}");
        }
    }
    
    public void InsertTextAsImageToPdf(string pdfPath,string outPath, string text,int pageNum, float llx, float lly, float urx, float ury)
    {
        // Generate an image from the string
        using var image = new Image<Rgba32>(1, 1);
        var font = SystemFonts.CreateFont("Times New Roman", 30, FontStyle.Regular);
        var textOptions = new RichTextOptions(font)
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        var textSize = TextMeasurer.MeasureSize(text, textOptions);
        image.Mutate(ctx => ctx.Resize((int)textSize.Width + 10, (int)textSize.Height + 10));
        image.Mutate(ctx => ctx.DrawText(textOptions, text, Color.Black));

        // Save the image to a memory stream
        using var imageStream = new MemoryStream();
        image.SaveAsPng(imageStream);
        imageStream.Position = 0;

        // Load the PDF document
        using var pdfDocument = new PdfLoadedDocument(pdfPath);
        var page = pdfDocument.Pages[pageNum-1];

        // Calculate the width and height of the image based on the coordinates
        var width = urx - llx;
        var height = ury - lly;

        // Load the image into the PDF
        var pdfImage = new PdfBitmap(imageStream);
        page.Graphics.DrawImage(pdfImage, llx, page.Size.Height - ury, width, height);

        // Save the updated PDF
        // outPath = Path.Combine(outPath,Path.GetFileName(pdfPath));
        pdfDocument.Save(outPath);
        pdfDocument.Close(true);
    }
    
    public IActionResult InsertTextToImage(IFormFile file, string text)
    {
        var avatarFolder = Path.Combine(_storagePath, "tmp");
        Directory.CreateDirectory(avatarFolder);
    
        // Generate a unique file name for the uploaded file
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        var imagePath = Path.Combine(avatarFolder, fileName);
    
        // Save the uploaded file to the server
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
    
        // Load the uploaded image
        using var image = Image.Load<Rgba32>(imagePath);
        // Configure font
        Font font = SystemFonts.CreateFont("Times New Roman", 24, FontStyle.Regular);
    
        // Measure text size
        var textOptions = new TextOptions(font)
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            WrappingLength = image.Width
        };
        var textSize = TextMeasurer.MeasureSize(text, textOptions);
    
        int spacing = 10; // Space between image and text
        int newHeight = image.Height + (int)textSize.Height + spacing;
    
        // Create a new image with increased height to accommodate the text
        using var outputImage = new Image<Rgba32>(image.Width, newHeight);
        if (file.ContentType.ToLower() != "image/png")
        {
            File.Delete(imagePath);
            throw new ArgumentException("Invalid file format. Only PNG images are supported.");
        }

        if (image.Width <= 300 || image.Width >= 330 || image.Height <= 150 || image.Height >= 200)
        {
            File.Delete(imagePath);
            throw new ArgumentException("Image dimensions must be greater than 300x150 and smaller than 330x200.");
        }
        outputImage.Mutate(ctx =>
        {
            // Draw the original image
            ctx.Clear(Color.Transparent);
            ctx.DrawImage(image, new Point(0, 0), 1f);
    
            // Draw the text below the image, centered
            var position = new PointF(image.Width / 2f, image.Height + spacing - 5);
            ctx.DrawText(new RichTextOptions(font)
            {
                Origin = position,
                HorizontalAlignment = HorizontalAlignment.Center
            }, text, Color.Black);
        });
    
        // Save the new image
        var outputFileName = fileName;
        var outputPath = Path.Combine(avatarFolder, outputFileName);
        outputImage.Save(outputPath);
        // Return the new image as a file result
        var contentType = GetContentType(outputPath);
        var fileBytes = System.IO.File.ReadAllBytes(outputPath);
        File.Delete(outputPath);

        return new FileContentResult(fileBytes, contentType)
        {
            FileDownloadName = outputFileName
        };
    }

    public string CreateFirstVersion(Guid documentId,string documentName, Guid versionId, Guid templateId)
    {
        var templatePath = Path.Combine(_storagePath, "template", templateId.ToString()+".docx");
        var filePath = Path.Combine(_storagePath, "document", documentId.ToString(), versionId.ToString());
        // var templateFile = Directory.GetFiles(templatePath).FirstOrDefault(x => Path.GetExtension(x)==".docx");
        templatePath = Path.Combine(templatePath, templatePath);
        Directory.CreateDirectory(filePath);
        File.Copy(templatePath, Path.Combine(filePath, documentName + ".docx"));
        return _host + "/api/Document/view-file/" + documentId + "?version=0&isArchive=false&isDoc=true";
    }

    public async Task<string> InsertNumberDocument(IFormFile file,Guid templateId,string numberDoc,int pageNum,int llx,int lly, int urx, int ury)
    {
        var filePath = Path.Combine(_storagePath, "tmp", file.FileName);
        Directory.CreateDirectory(_storagePath);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        using (var wordDoc = WordprocessingDocument.Open(filePath, false)) // Open in read-only mode
        {
            var packageProperties = wordDoc.PackageProperties;
            var identifier = packageProperties.Subject;
            if(identifier is null || identifier != templateId.ToString()) 
            {
                throw new Exception("File không phải là mẫu");
            }
            File.Delete(filePath);
        }
        var path = await ConvertDocToPdfPrivate(file);
        InsertTextAsImageToPdf(path,path,numberDoc,pageNum,llx,lly,urx,ury);
        return path;
    }

    public async Task<string> SaveNewVersionDocFromBase64(DocumentCompareDto docCompareDto,DocumentVersion version)
    {
        var base64Data = docCompareDto.FileBase64.Contains(',') ? docCompareDto.FileBase64.Split(',')[1] : docCompareDto.FileBase64;
        var pdfBytes = Convert.FromBase64String(base64Data);
        var filepath = Path.Combine(_storagePath, "document", docCompareDto.DocumentId.ToString(), version.DocumentVersionId.ToString());
        var nameFile = docCompareDto.DocumentName + ".pdf";
        Directory.CreateDirectory(filepath);
        filepath = Path.Combine(filepath, nameFile);
        await File.WriteAllBytesAsync(filepath, pdfBytes);
        var url = _host + "/api/Document/view-file/" + docCompareDto.DocumentId + $"?version={version.VersionNumber}&isArchive=false";
        return url;
    }

    public async Task<IActionResult> GetAttachFileById(Guid documentId)
    {
        var doc = await _unitOfWork.AttachmentUOW.FindAttachmentDocumentByIdAsync(documentId);
        var path = Path.Combine(_storagePath, "tmpA");
        string searchPattern = $"{documentId}.*";
        var files = Directory.GetFiles(path, searchPattern);
        var filePath = files.FirstOrDefault();
        if (filePath == null || !File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        
        var contentType = GetContentType(filePath);
        var bytes = await File.ReadAllBytesAsync(filePath);
        return new FileContentResult(bytes, contentType)
        {
            FileDownloadName = doc.AttachmentDocumentName+ Path.GetExtension(filePath)
        };
    }


    private static string GetContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".pdf" => "application/pdf",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/octet-stream",
        };
    }
}