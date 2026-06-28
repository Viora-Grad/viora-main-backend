using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Viora.Infrastructure.Firebase;

public static class FirebaseDiService
{
    public static IServiceCollection AddFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        var firebaseSettings = new FirebaseSettings();
        configuration.GetSection("Firebase").Bind(firebaseSettings);

        if (string.IsNullOrEmpty(firebaseSettings.Path))
            throw new ArgumentException("Firebase service account credential path is not configured.");

        services.AddSingleton<FirebaseApp>(_ =>
            FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = CredentialFactory
                        .FromFile<ServiceAccountCredential>(firebaseSettings.Path)
                        .ToGoogleCredential()
            })
        );

        return services;
    }
    public static IServiceCollection AddFirebaseMessaging(this IServiceCollection services)
    {
        services.AddSingleton<FirebaseMessaging>(provider =>
        {
            var firebaseApp = provider.GetRequiredService<FirebaseApp>();
            return FirebaseMessaging.GetMessaging(firebaseApp);
        });
        return services;
    }
    // Add other Firebase services as needed, e.g., Firestore, Realtime Database, Authentication, etc.
}
