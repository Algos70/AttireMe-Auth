using System.Text.Json.Serialization;

namespace AuthenticationService.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<GetUserInfoOutcomes>))]
public enum GetUserInfoOutcomes
{
    Success,
    EmailNotFound,
    UserIsAdmin,
    UserNotInitialized,
    CreatorNotInitialized
}