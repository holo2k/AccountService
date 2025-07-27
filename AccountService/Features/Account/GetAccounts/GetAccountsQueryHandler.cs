using MediatR;

namespace AccountService.Features.Account.GetAccount
{
    public class GetAccountsQueryHandler : IRequestHandler<GetAccountsQuery, ICollection<AccountDto>>
    {
        public GetAccountsQueryHandler()
        {
            
        }

        public async Task<ICollection<AccountDto>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
