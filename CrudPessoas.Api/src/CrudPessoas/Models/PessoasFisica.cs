

namespace CrudPessoas.Models
{
    class PessoasFisica : Pessoas
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
    }
}
