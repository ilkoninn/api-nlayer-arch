namespace App.Business.DTOs.Contact;

public class UpdateContactStatusDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public EContactStatus Status { get; set; }
}