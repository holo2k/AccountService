﻿using MediatR;

namespace AccountService.Features.Transaction.AddTransaction;

public record AddTransactionCommand(TransactionDto Transaction) : IRequest<Guid>;