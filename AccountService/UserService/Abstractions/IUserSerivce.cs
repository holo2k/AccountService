using System.Text.Json;
using AccountService.PipelineBehaviors;

namespace AccountService.UserService.Abstractions;

public interface IUserService
{
    Task<bool> IsExistsAsync(Guid ownerId);
    Task<MbResult<JsonElement>> GetToken();
}