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
        var section = configuration.GetSection("Firebase");
        section.Bind(firebaseSettings);

        var serviceAccountJson = GetFirebaseServiceAccountJson(firebaseSettings);


        services.AddSingleton<FirebaseApp>(_ =>
            FirebaseApp.DefaultInstance ?? FirebaseApp.Create(new AppOptions
            {
                Credential = CredentialFactory
                        .FromJson<ServiceAccountCredential>(serviceAccountJson)
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

    private static string GetFirebaseServiceAccountJson(FirebaseSettings firebaseSettings)
    {
        var serviceAccount = new
        {
            type = firebaseSettings.Type,
            project_id = firebaseSettings.Project_Id,
            private_key_id = firebaseSettings.Private_Key_Id,
            private_key = firebaseSettings.Private_Key.Replace("\\n", "\n"),
            client_email = firebaseSettings.Client_Email,
            client_id = firebaseSettings.Client_Id,
            auth_uri = firebaseSettings.Auth_Uri,
            token_uri = firebaseSettings.Token_Uri,
            auth_provider_x509_cert_url = firebaseSettings.Auth_Provider_X509_Cert_Url,
            client_x509_cert_url = firebaseSettings.Client_X509_Cert_Url
        };
        return System.Text.Json.JsonSerializer.Serialize(serviceAccount);
    }
}
