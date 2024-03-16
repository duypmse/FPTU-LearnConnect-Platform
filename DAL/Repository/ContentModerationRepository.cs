using AutoMapper;
using BAL.Models;
using DAL.DTO;
using Google.Cloud.VideoIntelligence.V1;
using Google.Type;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IContentModerationRepository : IBaseRepository<ContentModerationDTO>
    {
        public Task<object> Moderation(int lectureId);
        public object GetModeration(int lectureId);
        public ContentModeration GetContentModeration(int lectureId);
        public bool SaveModerationData(ContentModeration contentModeration, List<FlagDetail> flags);

    }
    public class ContentModerationRepository : IContentModerationRepository
    {
        private readonly LearnConnectDBContext _context;
        private readonly IMapper _mapper;

        public ContentModerationRepository(LearnConnectDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public ContentModerationDTO Add(ContentModerationDTO _objectDTO)
        {
            var _object = _mapper.Map<ContentModeration>(_objectDTO);
            _context.ContentModerations.Add(_object);
            return null;
        }

        public ContentModerationDTO Get(int id)
        {
            var _object = _context.ContentModerations.Find(id);
            var _objectDTO = _mapper.Map<ContentModerationDTO>(_object);
            return _objectDTO;
        }

        public IEnumerable<ContentModerationDTO> GetList()
        {
            var _list = _context.ContentModerations.ToList();
            var _listDTO = _mapper.Map<List<ContentModerationDTO>>(_list);
            return _listDTO;
        }

        public int Update(int id, ContentModerationDTO _objectDTO)
        {
            var _object = _context.ContentModerations.Find(id);
            if (_object == null)
            {
                return 0;
            }
            _mapper.Map(_objectDTO, _object);

            _context.ContentModerations.Update(_object);
            return 1;
        }

        public int Delete(int id)
        {
            ContentModeration _object = _context.ContentModerations.Find(id);
            if (_object == null)
            {
                throw new Exception();
            }
            _context.ContentModerations.Remove(_object);
            return 1;
        }

        public int SaveChanges()
        {
            var _numOfChanges = _context.SaveChanges();
            return _numOfChanges;
        }

        public bool Exists(int id)
        {
            var _isExist = _context.ContentModerations.Any(e => e.Id == id);
            return _isExist;
        }

        //public string GetFirebaseStorageLocation(int lectureId)
        //{
        //    var lecture = _context.Lectures.Find(lectureId);
        //    if (lecture.ContentType == 0)
        //    {
        //        return null;
        //    }

        //    var fileName = lecture.ContentUrl.Substring(lecture.ContentUrl.IndexOf("Course")).Split('?')[0];
        //    var gcsUri = "gs://learnconnect-6f324.appspot.com/lectures/" + fileName;
        //    return gcsUri;
        //}

        public ContentModeration GetContentModeration(int lectureId)
        {
            var lecture = _context.Lectures.Find(lectureId);
            if (lecture.ContentType == 0)
            {
                return null;
            }

            var contentModeration = _context.ContentModerations.FirstOrDefault(c => c.LectureId == lectureId);
            if (contentModeration == null)
            {
                var contentModerationDTO = new ContentModerationDTO
                {
                    VideoUrl = lecture.ContentUrl,
                    ContentLength = lecture.LectureLength,
                    Status = (int)ContentModerationStatus.Pending,
                    LectureId = lecture.Id
                };
                contentModeration = _mapper.Map<ContentModeration>(contentModerationDTO);
                _context.ContentModerations.Add(contentModeration);
                _context.SaveChanges();
            }

            return contentModeration;
        }

        public bool SaveModerationData(ContentModeration contentModeration, List<FlagDetail> flags)
        {
            _context.ContentModerations.Update(contentModeration);

            _context.FlagDetails.AddRange(flags);

            _context.SaveChanges();

            return true;
        }

        public async Task<object> Moderation(int lectureId)
        {
            var lecture = _context.Lectures.Find(lectureId);
            if( lecture.ContentType == 0)
            {
                return "Wrong content type!";
            }

            var fileName = lecture.ContentUrl.Substring(lecture.ContentUrl.IndexOf("Course")).Split('?')[0];
            var gcsUri = "gs://learnconnect-6f324.appspot.com/lectures/" + fileName;

            var contentModeration = _context.ContentModerations.FirstOrDefault(c => c.LectureId== lectureId);
            if(contentModeration == null)
            {
                var contentModerationDTO = new ContentModerationDTO
                {
                    VideoUrl = lecture.ContentUrl,
                    ContentLength = lecture.LectureLength,
                    Status = (int)ContentModerationStatus.Pending,
                    LectureId = lecture.Id
                };
                contentModeration = _mapper.Map<ContentModeration>(contentModerationDTO);
                _context.ContentModerations.Add(contentModeration);
                _context.SaveChanges();
            }
            

            var unExplicitFrame = 0;
            var unSafeFrame = 0;
            var totalFrame = 0;
            List<FlagDetailDTO> flags = new List<FlagDetailDTO>();
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
                if (frame.PornographyLikelihood.ToString().Equals("POSSIBLE") 
                    || frame.PornographyLikelihood.ToString().Equals("LIKELY")
                    || frame.PornographyLikelihood.ToString().Equals("VERY_LIKELY"))
                {
                    unSafeFrame++;
                    var flagDetailDTO = new FlagDetailDTO
                    {
                        Title = "Adult Content",
                        Description = frame.PornographyLikelihood.ToString(),
                        AtTime = (int)frame.TimeOffset.Seconds,
                        ContentModerationId = contentModeration.Id
                    };

                    flags.Add(flagDetailDTO);
                    var flagDetail = _mapper.Map<FlagDetail>(flagDetailDTO);
                    _context.FlagDetails.Add(flagDetail);
                }
                if (frame.PornographyLikelihood.ToString().Equals("LIKELIHOOD_UNSPECIFIED"))
                {
                    unExplicitFrame++;
                }
            }

            contentModeration.PercentExplicit = (1-(decimal)unExplicitFrame / totalFrame)*100;
            contentModeration.PercentUnsafe = ((decimal)unSafeFrame / totalFrame) * 100;

            _context.ContentModerations.Update(contentModeration);
            _context.SaveChanges();

            return new
            {
                ContentModeration = contentModeration,
                Flags = flags
            };
        }

        public object GetModeration(int lectureId)
        {
            var lecture = _context.Lectures.Find(lectureId);
            if (lecture.ContentType == 0)
            {
                return "Wrong content type!";
            }

            var contentModeration = _context.ContentModerations.FirstOrDefault(c => c.LectureId == lectureId);
            var flags = _context.FlagDetails.Where(f => f.ContentModerationId == contentModeration.Id).ToList();

            return new
            {
                ContentModeration = contentModeration,
                Flags = flags
            };
        }
    }
}
