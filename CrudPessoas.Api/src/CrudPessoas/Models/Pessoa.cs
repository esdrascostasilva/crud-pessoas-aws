using Amazon.DynamoDBv2.DataModel;

namespace CrudPessoas.Models
{   
    [DynamoDBTable("Pessoas")]
    public class Pessoa
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty]
        public string Nome { get; set; }

        [DynamoDBProperty]
        public string Documento { get; set; }

        [DynamoDBProperty]
        public string Cep { get; set; }

        [DynamoDBProperty]
        public string? Endereco { get; set; }

        [DynamoDBProperty]
        public bool PessoaFisica { get; set; }

        [DynamoDBProperty]
        public bool PessoaJuridica { get; set; }
    }
}
