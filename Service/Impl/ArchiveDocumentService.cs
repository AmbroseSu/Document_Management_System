using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using iText.Signatures;
//using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Repository;
using Syncfusion.Pdf.Parsing;
using PdfDocument = iText.Kernel.Pdf.PdfDocument;
using PdfReader = iText.Kernel.Pdf.PdfReader;

namespace Service.Impl;

public class ArchiveDocumentService : IArchiveDocumentService
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveDocumentService(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }
    
    /*public string ExtractSignatures(IFormFile file)
{
    if (file == null || file.Length == 0)
    {
        throw new ArgumentException("File không hợp lệ.");
    }

    using (var memoryStream = new MemoryStream())
    {
        // Đọc nội dung file vào MemoryStream
        file.OpenReadStream().CopyTo(memoryStream);

        // Đảm bảo rằng chúng ta có thể đọc lại từ MemoryStream
        memoryStream.Position = 0;

        try
        {
            // Mở tài liệu PDF bằng Syncfusion
            using (PdfLoadedDocument loadedDocument = new PdfLoadedDocument(memoryStream))
            {
                var signatureDetails = new List<string>();

                // Lặp qua tất cả các chữ ký trong tài liệu PDF
                foreach (var signature in loadedDocument.Signatures)
                {
                    if (signature is PdfSignature pdfSignature)
                    {
                        // Kiểm tra trạng thái chữ ký
                        if (pdfSignature.SignatureVerificationStatus == SignatureVerificationStatus.Signed)
                        {
                            // Thêm thông tin chữ ký vào danh sách
                            string signatureInfo = $"Signed By: {pdfSignature.SignerName}, " +
                                                   $"Signed Date: {pdfSignature.SigningDate}, " +
                                                   $"Valid: {pdfSignature.IsValid}";

                            // Lấy thông tin chứng chỉ
                            X509Certificate2 cert = pdfSignature.Certificate;
                            signatureInfo += $", Cert Subject: {cert.Subject}, " +
                                             $"Cert Issuer: {cert.Issuer}, " +
                                             $"Valid From: {cert.NotBefore}, " +
                                             $"Valid To: {cert.NotAfter}";

                            signatureDetails.Add(signatureInfo);
                        }
                    }
                }

                // Trả về thông tin chữ ký dưới dạng chuỗi
                return string.Join(Environment.NewLine, signatureDetails);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Không thể xử lý PDF: {ex.Message}");
        }
    }
}*/

}
