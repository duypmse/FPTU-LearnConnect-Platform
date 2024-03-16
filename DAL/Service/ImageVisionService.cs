using BAL.Models;
using DAL.Repository;
using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DAL.Service
{
    public interface IImageVisionService
    {
        public Task<object> ScanBack(string imagePath);
        public Task<string> ScanFront(string imagePath);
    }
    public class ImageVisionService : IImageVisionService
    {
        private readonly IVerificationDocumentRepository repo;

        public ImageVisionService(IVerificationDocumentRepository repo)
        {
            this.repo = repo;
        }

        public async Task<object> ScanBack(string videoUrl)
        {
            //var contentModeration = repo.Get(130);

            //var videoUrl = contentModeration.DocumentUrl;
            var fileName = videoUrl.Substring(videoUrl.IndexOf("images%2F") + 9).Split('?')[0];
            var imagePath = "gs://learnconnect-6f324.appspot.com/images/" + fileName;

            var client = ImageAnnotatorClient.Create();
            var image = Image.FromUri(imagePath);
            var response = client.DetectText(image);
            var data = response[0].Description;

            string createDate = "";
            string cardId = "";
            string fullname = "";

            string pattern = @"\d{2}/\d{2}/\d{4}";
            Match match = Regex.Match(data, pattern);
            if(match.Success)
            {
                createDate = match.Value;
            }

            var tmp = data.IndexOf('<');
            cardId = data.Substring(tmp - 12, 12);

            var tmp2 = data.LastIndexOf('<');
            var stringHasName = data.Substring(tmp, tmp2-tmp).Split('\n')[2];
            //fullname = new string(Array.FindAll(stringHasName.ToCharArray(), char.IsUpper));
            fullname = stringHasName.Replace("<"," ").Trim().Replace("  "," ");

            return new
            {
                fullname,
                cardId,
                createDate,
            };
        }

        public async Task<string> ScanFront(string imagePath)
        {
            var client = ImageAnnotatorClient.Create();
            var image = Image.FromUri(imagePath);
            var response = client.DetectText(image);
            string returnData = ExtractDigits(response[0].Description);
            return returnData;
        }
        private static string ExtractDigits(string input)
        {
            var digits = input;

            for (int i = 0; i <= digits.Length - 12; i++)
            {
                string consecutiveDigits = digits.Substring(i, 12);

                if (Is12Digits(consecutiveDigits))
                {
                    return consecutiveDigits;
                }
            }

            return string.Empty;
        }

        private static bool Is12Digits(string input)
        {
            var charInput = input.ToCharArray();
            for (int i = 0; i < charInput.Length - 1; i++)
            {
                if (!char.IsDigit(charInput[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
