using MediatR;

namespace AccountService.Features.Account.AddAccount
{
    public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, AccountDto>
    {
        public AddAccountCommandHandler()
        {
            
        }

        public async Task<AccountDto> Handle(AddAccountCommand request, CancellationToken cancellationToken)
        {


            return request.Account;
        }
    }
}
