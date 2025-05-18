// IStreamChatService.cs
using System.Threading.Tasks;

namespace AuthenticationService.Interfaces.Services
{
    public interface IStreamChatService
    {
        Task<string> GenerateTokenAsync(string userId);
    }
}