using DAL.Repository;
using Firebase.Auth;
using Firebase.Storage;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Service
{
    public interface IFirebaseService
    {
        public Task<string> Upload(IFormFile file, string fileName, string action);
        public Task<object> LoginAsync(string accessToken);
    }
    public class FirebaseService : IFirebaseService
    {
        public IConfiguration Configuration { get; }
        private readonly IUserRepository repo;

        public FirebaseService(IConfiguration configuration, IUserRepository repo)
        {
            Configuration = configuration;
            this.repo = repo;
        }

        public async Task<string> Upload(IFormFile file, string fileName, string action)
        {
            string url;

            using (var stream = file.OpenReadStream())
            {

                var auth = new FirebaseAuthProvider(new FirebaseConfig(Configuration["ConfigUpload:Apikey"]));
                var a = await auth.SignInWithEmailAndPasswordAsync(Configuration["ConfigUpload:AuthEmail"], Configuration["ConfigUpload:AuthPassword"]);

                var cancellation = new CancellationTokenSource();
                var bucket = Configuration["ConfigUpload:Bucket"];

                var task = new FirebaseStorage(
                    bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true,
                    });

                url = await task
                    .Child(action)
                    .Child(fileName)
                    .PutAsync(stream, cancellation.Token);
            }

            return url;
        }

        public async Task<object> LoginAsync( string accessToken)
        {
            var auth = FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance;
            FirebaseToken decodedToken = await auth.VerifyIdTokenAsync(accessToken);

            var email = decodedToken.Claims["email"].ToString();

            var staffMails = repo.GetStaffMail();

            /*if (!email.EndsWith("@fpt.edu.vn"))
            {
                if(!(staffMails.Contains(email) || email.Equals(Configuration["MailManager"])))
                {
                    return "Only @fpt.edu.vn email addresses are allowed!";
                }
            }*/

            UserRecord userRecord = await auth.GetUserAsync(decodedToken.Uid);

            return userRecord;
        }
    }
}
