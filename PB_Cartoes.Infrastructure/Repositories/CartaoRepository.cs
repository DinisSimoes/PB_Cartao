using PB_Cartoes.Domain.Entities;
using PB_Cartoes.Domain.Interfaces;
using PB_Cartoes.Infrastructure.Data;

namespace PB_Cartoes.Infrastructure.Repositories
{
    public class CartaoRepository : ICartaoRepository
    {
        private readonly CartoesContext _context;

        public CartaoRepository(CartoesContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Cartao cartao)
        {
            _context.Cartoes.Add(cartao);
            await _context.SaveChangesAsync();
        }
    }
}