using AccountService.Features.Account;
using AccountService.Features.Account.AddAccount;
using AccountService.Features.Transaction;
using AutoMapper;

namespace AccountService.AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>()
            .ReverseMap();

        CreateMap<Transaction, TransactionDto>()
            .ReverseMap();

        CreateMap<AddAccountRequest, Account>()
            .ReverseMap();
    }
}