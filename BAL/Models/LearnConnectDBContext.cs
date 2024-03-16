using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BAL.Models
{
    public partial class LearnConnectDBContext : DbContext
    {
        public LearnConnectDBContext()
        {
        }

        public LearnConnectDBContext(DbContextOptions<LearnConnectDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Answer> Answers { get; set; } = null!;
        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<ContentModeration> ContentModerations { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;
        public virtual DbSet<FavoriteCourse> FavoriteCourses { get; set; } = null!;
        public virtual DbSet<FlagDetail> FlagDetails { get; set; } = null!;
        public virtual DbSet<LearningProcessDetail> LearningProcessDetails { get; set; } = null!;
        public virtual DbSet<Lecture> Lectures { get; set; } = null!;
        public virtual DbSet<Major> Majors { get; set; } = null!;
        public virtual DbSet<Mentor> Mentors { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<Rating> Ratings { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<Specialization> Specializations { get; set; } = null!;
        public virtual DbSet<SpecializationOfMentor> SpecializationOfMentors { get; set; } = null!;
        public virtual DbSet<Test> Tests { get; set; } = null!;
        public virtual DbSet<TestResult> TestResults { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserAnswer> UserAnswers { get; set; } = null!;
        public virtual DbSet<VerificationDocument> VerificationDocuments { get; set; } = null!;

        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();
            var strConn = config["ConnectionStrings:MyDB"];

            return strConn;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.ToTable("Answer");

                entity.Property(e => e.AnswerText).IsUnicode(false);

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Answers)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Answer__Question__52593CB8");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("Comment");

                entity.Property(e => e.Comment1)
                    .IsUnicode(false)
                    .HasColumnName("Comment");

                entity.Property(e => e.CommentTime).HasColumnType("datetime");

                entity.HasOne(d => d.Lecture)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.LectureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Comment__Lecture__6EF57B66");
            });

            modelBuilder.Entity<ContentModeration>(entity =>
            {
                entity.ToTable("ContentModeration");

                entity.HasIndex(e => e.LectureId, "UQ__ContentM__B739F6BE03B8AA16")
                    .IsUnique();

                entity.Property(e => e.PercentExplicit).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.PercentUnsafe).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.PreviewDate).HasColumnType("datetime");

                entity.Property(e => e.RejectReason)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.VideoUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Lecture)
                    .WithOne(p => p.ContentModeration)
                    .HasForeignKey<ContentModeration>(d => d.LectureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ContentMo__Lectu__412EB0B6");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.Property(e => e.AverageRating).HasColumnType("decimal(3, 2)");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Note).IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ShortDescription).IsUnicode(false);

                entity.HasOne(d => d.Mentor)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.MentorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Course__MentorId__3A81B327");

                entity.HasOne(d => d.Specialization)
                    .WithMany(p => p.Courses)
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Course__Speciali__398D8EEE");
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.ToTable("Enrollment");

                entity.Property(e => e.EnrollmentDate).HasColumnType("datetime");

                entity.Property(e => e.PercentComplete).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.TotalScore).HasColumnType("decimal(5, 2)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Enrollmen__Cours__628FA481");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Enrollments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Enrollmen__UserI__619B8048");
            });

            modelBuilder.Entity<FavoriteCourse>(entity =>
            {
                entity.ToTable("FavoriteCourse");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.FavoriteCourses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FavoriteC__UserI__68487DD7");
            });

            modelBuilder.Entity<FlagDetail>(entity =>
            {
                entity.ToTable("FlagDetail");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContentModeration)
                    .WithMany(p => p.FlagDetails)
                    .HasForeignKey(d => d.ContentModerationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__FlagDetai__Conte__440B1D61");
            });

            modelBuilder.Entity<LearningProcessDetail>(entity =>
            {
                entity.ToTable("LearningProcessDetail");

                entity.HasOne(d => d.Enrollment)
                    .WithMany(p => p.LearningProcessDetails)
                    .HasForeignKey(d => d.EnrollmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__LearningP__Enrol__6B24EA82");

                entity.HasOne(d => d.Lecture)
                    .WithMany(p => p.LearningProcessDetails)
                    .HasForeignKey(d => d.LectureId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__LearningP__Lectu__6C190EBB");
            });

            modelBuilder.Entity<Lecture>(entity =>
            {
                entity.ToTable("Lecture");

                entity.Property(e => e.Content).IsUnicode(false);

                entity.Property(e => e.ContentUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.RejectReason)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Lectures)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Lecture__CourseI__3D5E1FD2");
            });

            modelBuilder.Entity<Major>(entity =>
            {
                entity.ToTable("Major");

                entity.Property(e => e.Description)
                    .HasMaxLength(3000)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Mentor>(entity =>
            {
                entity.ToTable("Mentor");

                entity.HasIndex(e => e.UserId, "UQ__Mentor__1788CC4D1BEC82E1")
                    .IsUnique();

                entity.Property(e => e.AverageRating).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Description)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.PaypalAddress)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PaypalId)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Mentor)
                    .HasForeignKey<Mentor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Mentor__UserId__286302EC");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notification");

                entity.Property(e => e.Description)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__UserI__36B12243");
            });

            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.ToTable("PaymentTransaction");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.PaymentUrl).IsUnicode(false);

                entity.Property(e => e.SuccessDate).HasColumnType("datetime");

                entity.Property(e => e.TransactionError)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Enrollment)
                    .WithMany(p => p.PaymentTransactions)
                    .HasForeignKey(d => d.EnrollmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__PaymentTr__Enrol__656C112C");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("Question");

                entity.Property(e => e.QuestionText).IsUnicode(false);

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Question__TestId__4F7CD00D");
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("Rating");

                entity.Property(e => e.Comment)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Rating1)
                    .HasColumnType("decimal(3, 2)")
                    .HasColumnName("Rating");

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Rating__CourseId__5CD6CB2B");

                entity.HasOne(d => d.Mentor)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.MentorId)
                    .HasConstraintName("FK__Rating__MentorId__5DCAEF64");

                entity.HasOne(d => d.RatingByNavigation)
                    .WithMany(p => p.Ratings)
                    .HasForeignKey(d => d.RatingBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Rating__RatingBy__5BE2A6F2");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("Report");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.ImageUrl).IsUnicode(false);

                entity.Property(e => e.ReportType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TimeStamp).HasColumnType("datetime");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Report__CourseId__47DBAE45");

                entity.HasOne(d => d.Mentor)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.MentorId)
                    .HasConstraintName("FK__Report__MentorId__48CFD27E");

                entity.HasOne(d => d.ReportByNavigation)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.ReportBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Report__ReportBy__46E78A0C");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Note)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Schedule__UserId__71D1E811");
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.ToTable("Specialization");

                entity.Property(e => e.Description)
                    .HasMaxLength(3000)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Major)
                    .WithMany(p => p.Specializations)
                    .HasForeignKey(d => d.MajorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Specializ__Major__2D27B809");
            });

            modelBuilder.Entity<SpecializationOfMentor>(entity =>
            {
                entity.ToTable("SpecializationOfMentor");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Note).IsUnicode(false);

                entity.Property(e => e.VerificationDate).HasColumnType("datetime");

                entity.HasOne(d => d.Mentor)
                    .WithMany(p => p.SpecializationOfMentors)
                    .HasForeignKey(d => d.MentorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Specializ__Mento__30F848ED");

                entity.HasOne(d => d.Specialization)
                    .WithMany(p => p.SpecializationOfMentors)
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Specializ__Speci__300424B4");
            });

            modelBuilder.Entity<Test>(entity =>
            {
                entity.ToTable("Test");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.Note).IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Tests)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Test__CourseId__4CA06362");
            });

            modelBuilder.Entity<TestResult>(entity =>
            {
                entity.ToTable("TestResult");

                entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");

                entity.HasOne(d => d.Test)
                    .WithMany(p => p.TestResults)
                    .HasForeignKey(d => d.TestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TestResul__TestI__5629CD9C");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.TestResults)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__TestResul__UserI__5535A963");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Email, "UQ__User__A9D1053444640725")
                    .IsUnique();

                entity.Property(e => e.BioDescription)
                    .HasMaxLength(4000)
                    .IsUnicode(false);

                entity.Property(e => e.Dob).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FullName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.LastLoginDate).HasColumnType("datetime");

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ProfilePictureUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.RegistrationDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserAnswer>(entity =>
            {
                entity.ToTable("UserAnswer");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.HasOne(d => d.TestResult)
                    .WithMany(p => p.UserAnswers)
                    .HasForeignKey(d => d.TestResultId)
                    .HasConstraintName("FK__UserAnswe__TestR__59063A47");
            });

            modelBuilder.Entity<VerificationDocument>(entity =>
            {
                entity.ToTable("VerificationDocument");

                entity.Property(e => e.Description)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.DocumentUrl)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Mentor)
                    .WithMany(p => p.VerificationDocuments)
                    .HasForeignKey(d => d.MentorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Verificat__Mento__33D4B598");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
