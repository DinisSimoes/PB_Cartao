using PB_Cartoes.Domain.Entities;

namespace PB_Cartoes.Domain.Interfaces
{
    public interface ICartaoRepository
    {
        Task AddAsync(Cartao cartao);
    }
}
