using MediatR;

namespace AccountService.Features.Transaction.AddTransaction
{
    public class AddTransactionCommandHandler : IRequestHandler<AddTransactionCommand, TransactionDto>
    {
        public AddTransactionCommandHandler()
        {
            
        }

        public Task<TransactionDto> Handle(AddTransactionCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
