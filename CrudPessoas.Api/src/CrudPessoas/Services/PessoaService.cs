using System.Text.Json;
using System.Text.RegularExpressions;
using CrudPessoas.Exceptions;
using CrudPessoas.Models;
using CrudPessoas.Repositories;

namespace CrudPessoas.Services
{
    public class PessoaService
    {
        private readonly PessoaRepository _repository = new();

        public Task<IEnumerable<Pessoa>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Pessoa?> GetByIdAsync(string id) => _repository.GetByIdAsync(id);

        public async Task<Pessoa> CreateAsync(Pessoa pessoa)
        {
            Console.WriteLine($"[CreateAsync] Documento recebido: '{pessoa.Documento}'");

            if (!DocumentoValido(pessoa.Documento))
            {
                Console.WriteLine("[CreateAsync] Documento inválido detectado");
                throw new DocumentoInvalidoException(pessoa.Documento);;
            }

            var alreadyDocumentExist = await _repository.GetByDocumentoAsync(pessoa.Documento);

            if (alreadyDocumentExist != null)
                throw new DocumentoDuplicadoException(pessoa.Documento);

            pessoa = await PreencherEnderecoAsync(pessoa);

            await _repository.AddAsync(pessoa);
            Console.WriteLine("[CreateAsync] Pessoa adicionada com sucesso");
            return pessoa;
        }

        public async Task<Pessoa?> UpdateAsync(Pessoa pessoa)
        {
            var existing = await _repository.GetByIdAsync(pessoa.Id);
            if (existing == null) return null;

            if (!DocumentoValido(pessoa.Documento))
                throw new DocumentoInvalidoException(pessoa.Documento);

            var alreadyDocumentExist = await _repository.GetByDocumentoAsync(pessoa.Documento);
            if (alreadyDocumentExist != null && alreadyDocumentExist.Id != pessoa.Id)
                throw new DocumentoDuplicadoException(pessoa.Documento);

            pessoa = await PreencherEnderecoAsync(pessoa);

            await _repository.UpdateAsync(pessoa);
            return pessoa;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return false;

            await _repository.DeleteAsync(id);
            return true;
        }

        public async Task<Pessoa> PreencherEnderecoAsync(Pessoa pessoa)
        {
            if (string.IsNullOrWhiteSpace(pessoa.Cep))
                return pessoa;

            using var client = new HttpClient();
            var response = await client.GetAsync($"https://viacep.com.br/ws/{pessoa.Cep}/json/");

            if (!response.IsSuccessStatusCode)
                throw new CepIncorretoException(pessoa.Cep);

            //testando o retorno da API
            var conteudoViaCep = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Conteudo do retorno do ViaCep {conteudoViaCep}");

            var endereco = JsonSerializer.Deserialize<ViaCepResponse>(
                conteudoViaCep,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (endereco == null)
                throw new CepIncorretoException(pessoa.Cep);

            if (endereco.Erro)
                throw new CepIncorretoException(pessoa.Cep);

            if (!string.IsNullOrWhiteSpace(endereco.Logradouro))
                {
                    pessoa.Endereco = $"{endereco.Logradouro} - {endereco.Bairro} - {endereco.Localidade}/{endereco.Uf}";
                }
            
            return pessoa;
        }

        private bool DocumentoValido(string documento)
        {
            Console.WriteLine($"[DocumentoValido] Documento: {documento}");

            if (string.IsNullOrWhiteSpace(documento))
                return false;

            documento = Regex.Replace(documento, @"[^\d]", "");
            Console.WriteLine($"[DocumentoValido] Documento limpo: {documento}");

            if (documento.Length == 11)
                return ValidaCPF(documento);
            else if (documento.Length == 14)
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