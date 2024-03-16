using BAL.Models;
using DAL.Mapper;
using DAL.Repository;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DAL.DTO;
using DAL.Service;

var builder = WebApplication.CreateBuilder(args);

//Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("LearnConnectCORS", policy =>
                      {
                          policy.WithOrigins("*").AllowAnyHeader()
                                                 .AllowAnyMethod()
                                                 .AllowAnyOrigin();
                      });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LearnConnectDBContext>(options =>
{
    //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddScoped<UploadRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<ISpecializationRepository, SpecializationRepository>();
builder.Services.AddScoped<IContentModerationRepository, ContentModerationRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ISpecializationOfMentorRepository, SpecializationOfMentorRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IFavoriteCourseRepository, FavoriteCourseRepository>();
builder.Services.AddScoped<IFlagDetailRepository, FlagDetailRepository>();
builder.Services.AddScoped<ILectureRepository, LectureRepository>();
builder.Services.AddScoped<IMentorRepository, MentorRepository>();
builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ITestRepository, TestRepository>();
builder.Services.AddScoped<IUserAnswerRepository, UserAnswerRepository>();
builder.Services.AddScoped<IVerificationDocumentRepository, VerificationDocumentRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IMajorRepository, MajorRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IScheduleRepository, ScheduleRepository>();
builder.Services.AddScoped<IVideoIntelligenceService, VideoIntelligenceService>();
builder.Services.AddScoped<IFirebaseService, FirebaseService>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IVNPayService, VNPayService>();
builder.Services.AddScoped<IImageVisionService, ImageVisionService>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();


IConfiguration config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .Build();

var secretKey = config["JWT:Key"];
var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth_API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token with Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
});

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

var pathToKey = Path.Combine(Directory.GetCurrentDirectory(), "learnconnect_adminsdk.json");
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile(pathToKey)
});
System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToKey);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //Config Deploy
    app.UseDeveloperExceptionPage();
}
app.UseSwaggerUI();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
app.UseSwagger();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("LearnConnectCORS");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
