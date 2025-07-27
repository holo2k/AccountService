using MediatR;

namespace AccountService.Features.Account.DeleteAccount
{
    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Guid>
    {
        public DeleteAccountCommandHandler()
        {

        }

        public async Task<Guid> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            return request.AccountId;
        }
    }
}
