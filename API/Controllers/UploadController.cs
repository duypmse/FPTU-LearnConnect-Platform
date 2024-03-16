using DAL.Repository;
using Microsoft.AspNetCore.Authorization;
//using Firebase.Auth;
//using Firebase.Storage;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Services;
//using Google.Apis.Upload;
//using Google.Apis.Util.Store;
//using Google.Apis.YouTube.v3;
//using Google.Apis.YouTube.v3.Data;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [Route("api/upload")]
    [ApiController]
    //[Authorize]
    public class UploadController : ControllerBase
    {
        private IConfiguration Configuration { get; }
        private readonly UploadRepository repo;
        private static List<byte[]> receivedChunks = new List<byte[]>();

        public UploadController(UploadRepository repo, IConfiguration configuration)
        {
            Configuration = configuration;
            this.repo = repo;
        }

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var url = repo.Upload(file, file.FileName, "images").Result;

                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("video")]
        public async Task<ActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                var url = repo.Upload(file, file.FileName, "videos").Result;

                return Ok(url);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        //[HttpPost("uploadYoutube")]
        //public async Task<IActionResult> UploadVideoToYouTube(IFormFile file)
        //{
        //    UserCredential credential;
        //    using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
        //    {
        //        string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        //        credPath = Path.Combine(credPath, ".credentials/youtube-dotnet-quickstart.json");

        //        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //            GoogleClientSecrets.Load(stream).Secrets,
        //            new[] { YouTubeService.Scope.YoutubeUpload },
        //            "user",
        //            CancellationToken.None,
        //            new FileDataStore(credPath, true)).Result;
        //    }

        //    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        //    {
        //        HttpClientInitializer = credential,
        //        ApplicationName = "YouTube Upload"
        //    });

        //    var video = new Video();
        //    video.Snippet = new VideoSnippet
        //    {
        //        Title = "Your Video Title",
        //        Description = "Your Video Description",
        //        Tags = new[] { "Tag1", "Tag2" },
        //        CategoryId = "22" // Music category, you can change it to your desired category
        //    };
        //    video.Status = new VideoStatus
        //    {
        //        PrivacyStatus = "public" // You can change this to unlisted or private if needed
        //    };

        //    using (var  stream = file.OpenReadStream())
        //    {
        //        var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", stream, "video/*");
        //        videosInsertRequest.ProgressChanged += (IUploadProgress progress) =>
        //        {
        //            switch (progress.Status)
        //            {
        //                case UploadStatus.Uploading:
        //                    Console.WriteLine($"Uploading {progress.BytesSent} bytes...");
        //                    break;
        //                case UploadStatus.Completed:
        //                    Console.WriteLine("Video upload completed!");
        //                    break;
        //                case UploadStatus.Failed:
        //                    Console.Error.WriteLine("Video upload failed.");
        //                    break;
        //            }
        //        };

        //        await videosInsertRequest.UploadAsync();
        //    }

        //    return Ok("Video uploaded to YouTube!");
        //}

        [HttpPost("uploadChunk")]
        public async Task<IActionResult> UploadChunk([FromForm] byte[] chunkData)
        {
            // Xử lý chunk tại đây (lưu trữ hoặc xử lý dữ liệu)
            receivedChunks.Add(chunkData);
            // Trả về phản hồi cho client (ví dụ: số chunk đã nhận)
            return Ok(new { ChunkReceived = true });
        }

        [HttpPost("completeUpload")]
        public IActionResult CompleteUpload()
        {
            // Gộp các chunk đã nhận thành tệp hoàn chỉnh
            byte[] byteArray = CombineChunks(receivedChunks);
            var completeFile = ConvertByteArrayToIFormFile(byteArray, "Test 2", "video/mp4");

            var url = repo.Upload(completeFile, completeFile.FileName, "videos").Result;

            return Ok(url);
            // Lưu trữ hoặc xử lý tệp hoàn chỉnh theo nhu cầu của bạn
            // Ở đây, chúng ta chỉ trả về thông báo đã hoàn thành
        }

        private byte[] CombineChunks(List<byte[]> chunks)
        {
            // Tính toán kích thước của tệp hoàn chỉnh
            int totalSize = chunks.Sum(chunk => chunk.Length);
            byte[] combinedFile = new byte[totalSize];

            int offset = 0;
            foreach (var chunk in chunks)
            {
                chunk.CopyTo(combinedFile, offset);
                offset += chunk.Length;
            }

            return combinedFile;
        }

        private IFormFile ConvertByteArrayToIFormFile(byte[] bytes, string fileName, string contentType)
        {
            var stream = new MemoryStream(bytes);
            var formFile = new FormFile(stream, 0, stream.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType,
                ContentDisposition = $"form-data; name=file; filename={fileName}"
            };

            return formFile;
        }
    }
}
