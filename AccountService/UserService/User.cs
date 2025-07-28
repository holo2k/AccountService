using AccountService.Features.Account;

namespace AccountService.UserService;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
}