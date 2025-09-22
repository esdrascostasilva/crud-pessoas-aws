using System.Text.Json;
using System.Text.RegularExpressions;
using CrudPessoas.Models;
using CrudPessoas.Repositories;

namespace CrudPessoas.Services
{
    public class PessoaService
    {
        private readonly PessoaRepository _repo = new();

        public Task<IEnumerable<Pessoa>> GetAllAsync() => _repo.GetAllAsync();

        public Task<Pessoa?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);

        public async Task<Pessoa> CreateAsync(Pessoa pessoa)
        {
            if (!DocumentoValido(pessoa.Documento))
                throw new Exception("Documento inválido");

            pessoa = await PreencherEnderecoAsync(pessoa);

            await _repo.AddAsync(pessoa);
            return pessoa;
        }

        public async Task<Pessoa?> UpdateAsync(Pessoa pessoa)
        {
            var existing = await _repo.GetByIdAsync(pessoa.Id);
            if (existing == null) return null;

            if (!DocumentoValido(pessoa.Documento))
                throw new Exception("Documento inválido");

            pessoa = await PreencherEnderecoAsync(pessoa);

            await _repo.UpdateAsync(pessoa);
            return pessoa;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        public async Task<Pessoa> PreencherEnderecoAsync(Pessoa pessoa)
        {
            if (string.IsNullOrWhiteSpace(pessoa.Cep))
                return pessoa;

            using var client = new HttpClient();
            
            var response = await client.GetAsync($"https://viacep.com.br/ws/{pessoa.Cep}/json/");
            if (!response.IsSuccessStatusCode)
                return pessoa;

            //testando o retorno da API
            var conteudo = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Conteudo do retorno do ViaCep {conteudo}"); // veja exatamente o que está vindo do ViaCEP

            var endereco = JsonSerializer.Deserialize<ViaCepResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (endereco != null && !string.IsNullOrWhiteSpace(endereco.Logradouro))
            {
                pessoa.Endereco = $"{endereco.Logradouro} - {endereco.Bairro} - {endereco.Localidade}/{endereco.Uf}";
            }
            
            return pessoa;
        }

        private bool DocumentoValido(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = Regex.Replace(documento, @"[^\d]", ""); // remove caracteres não numéricos

            if (documento.Length == 11) // CPF
                return ValidaCPF(documento);
            else if (documento.Length == 14) // CNPJ
                return ValidaCNPJ(documento);

            return false;
        }

        private bool ValidaCPF(string cpf)
        {
            if (cpf.Length != 11)
                return false;

            if (new string(cpf[0], cpf.Length) == cpf)
                return false;

            int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * mult1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();

            tempCpf += digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * mult2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }

        private bool ValidaCNPJ(string cnpj)
        {
            if (cnpj.Length != 14)
                return false;

            if (new string(cnpj[0], cnpj.Length) == cnpj)
                return false;

            int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * mult1[i];

            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();

            tempCnpj += digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * mult2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cnpj.EndsWith(digito);
        }
    }
}