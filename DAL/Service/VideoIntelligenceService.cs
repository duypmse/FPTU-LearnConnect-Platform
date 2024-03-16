using BAL.Models;
using DAL.DTO;
using DAL.Repository;
using Google.Cloud.VideoIntelligence.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Service
{
    public interface IVideoIntelligenceService 
    {
        public Task<object> VideoModeration(int lectureId);
    }
    public class VideoIntelligenceService : IVideoIntelligenceService
    {
        private readonly IContentModerationRepository repo;

        public VideoIntelligenceService(IContentModerationRepository repo)
        {
            this.repo = repo;
        }

        public async Task<object> VideoModeration(int lectureId)
        {
            var contentModeration = repo.GetContentModeration(lectureId);

            var videoUrl = contentModeration.VideoUrl;
            var fileName = videoUrl.Substring(videoUrl.IndexOf("Course")).Split('?')[0];
            var gcsUri = "gs://learnconnect-6f324.appspot.com/lectures/" + fileName;

            var unExplicitFrame = 0;
            var unSafeFrame = 0;
            var totalFrame = 0;
            var flags = new List<FlagDetail>();
            // Instantiate a VideoIntelligenceServiceClient
            var client = await VideoIntelligenceServiceClient.CreateAsync();
            // Set the language code
            var context = new VideoContext
            {
                ExplicitContentDetectionConfig = new ExplicitContentDetectionConfig
                {
                    Model = "builtin/latest" // You can specify the model version here
                }
            };

            // Create the request with ExplicitContentDetection feature
            var request = new AnnotateVideoRequest
            {
                InputUri = gcsUri,
                Features = { Feature.ExplicitContentDetection },
                VideoContext = context
            };

            // Asynchronously perform explicit content detection on the video
            var response = await client.AnnotateVideoAsync(request);

            Console.WriteLine("Waiting for operation to complete...");

            var resultOperation = response.PollUntilCompleted();
            var result = resultOperation.Result.AnnotationResults[0];

            // Process and print explicit content detection results
            foreach (var frame in result.ExplicitAnnotation.Frames)
            {
                totalFrame++;
                if (frame.PornographyLikelihood.ToString().Equals("Possible")
                    || frame.PornographyLikelihood.ToString().Equals("Likely")
                    || frame.PornographyLikelihood.ToString().Equals("VeryLikely"))
                {
                    unSafeFrame++;
                    var flagDetail = new FlagDetail
                    {
                        Title = "Unsafe Content",
                        Description = frame.PornographyLikelihood.ToString(),
                        AtTime = (int)frame.TimeOffset.Seconds,
                        ContentModerationId = contentModeration.Id
                    };

                    flags.Add(flagDetail);
                }
                if (frame.PornographyLikelihood.ToString().Equals("LIKELIHOOD_UNSPECIFIED"))
                {
                    unExplicitFrame++;
                }
            }

            contentModeration.PercentExplicit = (1 - (decimal)unExplicitFrame / totalFrame) * 100;
            contentModeration.PercentUnsafe = ((decimal)unSafeFrame / totalFrame) * 100;

            repo.SaveModerationData(contentModeration, flags);
            return new
            {
                ContentModeration = contentModeration,
                Flags = flags
            };
        }
    }
}
