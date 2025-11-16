using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace khoaluantotnghiep.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment env, ILogger<UploadController> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Upload file chung (ảnh, tài liệu, ...)
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Không có file được chọn" });

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                    return BadRequest(new { message = "File không được vượt quá 10MB" });

                // Validate file type (only images for now)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                    return BadRequest(new { message = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, BMP)" });

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads");
                
                // Create uploads folder if not exists
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {fileName}");

                return Ok(new 
                { 
                    success = true,
                    fileName = fileName,
                    filePath = $"/uploads/{fileName}",
                    message = "Upload thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload file: {ex.Message}");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi upload file" });
            }
        }

        /// <summary>
        /// Upload nhiều file cùng lúc
        /// </summary>
        [HttpPost("multiple")]
        [Authorize]
        public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { message = "Không có file được chọn" });

                var uploadedFiles = new List<object>();
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "", "uploads");
                
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                foreach (var file in files)
                {
                    if (file.Length == 0) continue;

                    // Validate file size
                    if (file.Length > 10 * 1024 * 1024)
                    {
                        uploadedFiles.Add(new 
                        { 
                            originalName = file.FileName,
                            success = false,
                            message = "File vượt quá 10MB"
                        });
                        continue;
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    
                    if (!Array.Exists(allowedExtensions, ext => ext == fileExtension))
                    {
                        uploadedFiles.Add(new 
                        { 
                            originalName = file.FileName,
                            success = false,
                            message = "Chỉ chấp nhận file ảnh"
                        });
                        continue;
                    }

                    // Generate unique filename and save
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(new 
                    { 
                        originalName = file.FileName,
                        fileName = fileName,
                        filePath = $"/uploads/{fileName}",
                        success = true
                    });
                }

                return Ok(new 
                { 
                    success = true,
                    files = uploadedFiles,
                    message = $"Đã upload {uploadedFiles.Count} file"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi upload nhiều file: {ex.Message}");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi upload file" });
            }
        }
    }
}

