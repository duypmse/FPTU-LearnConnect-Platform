using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;


namespace DAL.Repository
{
    public class UploadRepository
    {
        private IConfiguration Configuration { get; }

        public UploadRepository(IConfiguration configuration)
        {
            Configuration = configuration;
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

        public async Task<string> UploadStream(Stream stream, string fileName, string action)
        {
            string url;

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

            return url;
        }
    }
}
