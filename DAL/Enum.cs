using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public enum Roles
    {
        Admin,
        Staff,
        Mentor,
        Student
    }

    public enum UserStatus
    {
        Active,
        Inactive
    }

    public enum TransactionStatus
    {
        Success,
        Error,
        Pending,
        Handled
    }

    public enum EnrollmentStatus
    {
        InProcessing,
        Canceled,
        Pending,
        Completed
    }

    public enum CourseStatus
    {
        Active,
        Pending,
        Reject,
        Banned,
        Inactive
    }
    
    public enum SpecializationStatus
    {
        Inactive,
        Active
    }

    public enum MajorStatus
    {
        Inactive,
        Active
    }

    public enum LectureStatus
    {
        Active,
        Pending,
        Reject,
        Banned,
        InActive
    }

    public enum MentorStatus
    {
        Active,
        Inactive
    }
    public enum SpecializationOfMentorStatus
    {
        Approve,
        Pending,
        Reject
    }
    public enum RatingStatus
    {
        Hide,
        Show
    }
    public enum LearningProcessStatus
    {
        Completed,
        InProcessing
    }
    public enum LearningProcessDetailStatus
    {
        Completed,
        InProcessing
    }
    public enum TestStatus
    {
        Active,
        Pending,
        Reject,
        Banned,
        Inactive
    }

    public enum ContentModerationStatus
    {
        Pending,
        Reject,
        Approve
    }

    public enum DocumentType
    {
        Specialization,
        Verification
    }

    public enum TransactionTypeStatus
    {
        Receive,
        Pay,
        SystemCommission
    }

    public enum CommentStatus
    {
        Comment,
        Reply
    }

    public enum SendNotiScheduleStatus
    {
        NotSent,
        SentStart,
        SentEnd,
        Sent
    }
}
