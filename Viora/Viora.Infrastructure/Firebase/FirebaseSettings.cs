namespace Viora.Infrastructure.Firebase;

internal class FirebaseSettings
{
    public string Type { get; set; } = null!;
    public string Project_Id { get; set; } = null!;
    public string Private_Key_Id { get; set; } = null!;
    public string Private_Key { get; set; } = null!;
    public string Client_Email { get; set; } = null!;
    public string Client_Id { get; set; } = null!;
    public string Auth_Uri { get; set; } = null!;
    public string Token_Uri { get; set; } = null!;
    public string Auth_Provider_X509_Cert_Url { get; set; } = null!;
    public string Client_X509_Cert_Url { get; set; } = null!;
    public string Universe_Domain { get; set; } = null!;
}
