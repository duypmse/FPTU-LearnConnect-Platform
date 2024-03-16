using AutoMapper;
using BAL;
using BAL.Models;
using DAL.DTO;

namespace DAL.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AnswerDTO, Answer>().ReverseMap();
            CreateMap<ContentModerationDTO, ContentModeration>().ReverseMap();
            CreateMap<CourseDTO, Course>().ReverseMap();
            CreateMap<EnrollmentDTO, Enrollment>().ReverseMap();
            CreateMap<FavoriteCourseDTO, FavoriteCourse>().ReverseMap();
            CreateMap<FlagDetailDTO, FlagDetail>().ReverseMap();
            CreateMap<LectureDTO, Lecture>().ReverseMap();
            CreateMap<MentorDTO, Mentor>().ReverseMap();
            CreateMap<PaymentTransactionDTO, PaymentTransaction>().ReverseMap();
            CreateMap<QuestionDTO, Question>().ReverseMap();
            CreateMap<RatingDTO, Rating>().ReverseMap();
            CreateMap<ReportDTO, Report>().ReverseMap();
            CreateMap<TestDTO, Test>().ReverseMap();
            CreateMap<UserAnswerDTO, UserAnswer>().ReverseMap();
            CreateMap<UserDTO, User>().ReverseMap();
            CreateMap<VerificationDocumentDTO, VerificationDocument>().ReverseMap();
            CreateMap<NotificationDTO, Notification>().ReverseMap();
            CreateMap<MajorDTO, Major>().ReverseMap();
            CreateMap<SpecializationDTO, Specialization>().ReverseMap();
            CreateMap<LearningProcessDetailDTO, LearningProcessDetail>().ReverseMap();
            CreateMap<SpecializationOfMentorDTO, SpecializationOfMentor>().ReverseMap();
            CreateMap<CommentDTO, Comment>().ReverseMap();
            CreateMap<ScheduleDTO, Schedule>().ReverseMap();
            CreateMap<TestResultDTO, TestResult>().ReverseMap();
        }
    }
}

