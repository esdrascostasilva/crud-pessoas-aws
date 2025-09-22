using Amazon.DynamoDBv2.DataModel;

namespace CrudPessoas.Models
{   
    [DynamoDBTable("Pessoas")]
    public class Pessoa
    {
        [DynamoDBHashKey]
        public string Id { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Nome { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Documento { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Cep { get; set; } = string.Empty;

        [DynamoDBProperty]
        public string Endereco { get; set; } = string.Empty;

        [DynamoDBProperty]
        public bool isPessoaFisica { get; set; }

        [DynamoDBProperty]
        public bool isPessoaJuridica { get; set; }
    }
}
