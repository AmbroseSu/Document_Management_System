using Azure;
using DataAccess.DTO;
using DocumentManagementSystemApplication.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace DocumentManagementSystemApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly ILoggingService _logService;
        private readonly string _toolsDirectory;

        public LogController(ILoggingService logService)
        {
            _logService = logService;
            _toolsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "data", "tools");
            // Tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(_toolsDirectory))
            {
                Directory.CreateDirectory(_toolsDirectory);
            }
        }
        
        
        [HttpGet("view-all-log")]
        [AuthorizeResource("[Log] View All Log")]
        public async Task<ResponseDto> ViewAllLog([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await _logService.GetAllLogsAsync(page, pageSize);
        }


        [HttpPost("update-upload")]
        [AuthorizeResource("[Log] Update Upload")]
        public async Task<IActionResult> UploadTool(IFormFile file)
        {
            try
            {
                // Kiểm tra file có được gửi lên không
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Không có file được gửi lên.");
                }

                // Kiểm tra định dạng file (ZIP)
                if (file.ContentType != "application/zip" &&
                    !file.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("File phải là định dạng ZIP.");
                }

                // Kiểm tra kích thước file (giới hạn 100MB)
                const long maxFileSize = 100 * 1024 * 1024; // 100MB
                if (file.Length > maxFileSize)
                {
                    return BadRequest("File vượt quá kích thước tối đa (100MB).");
                }

                // Đường dẫn lưu file
                var filePath = Path.Combine(_toolsDirectory, "tool.zip");

                // Lưu file vào thư mục
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { Message = "Upload file ZIP thành công.", FileName = "tool.zip" });
            }
            catch (IOException ex)
            {
                return StatusCode(500, $"Lỗi khi lưu file: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không mong muốn: {ex.Message}");
            }
        }
        
        [AllowAnonymous]
        [HttpGet("download")]
        public async Task<IActionResult> DownloadTool()
        {
            try
            {
                // Đường dẫn file ZIP
                var filePath = Path.Combine(_toolsDirectory, "tool.zip");

                // Kiểm tra file tồn tại
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("File ZIP không tồn tại trên server.");
                }

                // Đọc file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
                var fileName = "tool.zip";

                // Thiết lập header để tải file
                Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
                return File(fileBytes, "application/zip", fileName);
            }
            catch (IOException ex)
            {
                return StatusCode(500, $"Lỗi khi đọc file: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi không mong muốn: {ex.Message}");
            }
        }
    }

}