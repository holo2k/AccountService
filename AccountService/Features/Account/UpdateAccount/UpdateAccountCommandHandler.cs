using MediatR;

namespace AccountService.Features.Account.UpdateAccount
{
    public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, Guid>
    {
        public UpdateAccountCommandHandler()
        {
            
        }
        public async Task<Guid> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            return Guid.Empty;
        }
    }
}
