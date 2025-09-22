using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using CrudPessoas.Models;

namespace CrudPessoas.Repositories
{
    public class PessoaRepository
    {
        private readonly DynamoDBContext _context;

        public PessoaRepository()
        {
            var client = new AmazonDynamoDBClient();
            _context = new DynamoDBContext(client);
        }

        public async Task<IEnumerable<Pessoa>> GetAllAsync()
        {
            var conditions = new List<ScanCondition>();
            return await _context.ScanAsync<Pessoa>(conditions).GetRemainingAsync();
        }

        public async Task<Pessoa?> GetByIdAsync(string id)
        {
            return await _context.LoadAsync<Pessoa>(id);
        }

        public async Task AddAsync(Pessoa pessoa)
        {
            await _context.SaveAsync(pessoa);
        }

        public async Task UpdateAsync(Pessoa pessoa)
        {
            await _context.SaveAsync(pessoa);
        }

        public async Task DeleteAsync(string id)
        {
            var pessoa = await GetByIdAsync(id);

            if (pessoa != null)
                await _context.DeleteAsync(pessoa);
        }
    }
}