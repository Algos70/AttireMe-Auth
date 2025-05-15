namespace AuthenticationService.Interfaces;

public interface IGetUserResponse
{
    string Address { get; set; }
    string PhoneNumber { get; set; }
}