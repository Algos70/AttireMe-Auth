using System.Text.Json.Serialization;

namespace AuthenticationService.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<Roles>))]
public enum Roles
{
    Admin,
    User,
    Creator
}