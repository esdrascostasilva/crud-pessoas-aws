namespace CrudPessoas.Exceptions
{
    public class CepIncorretoException : Exception
    {
        public CepIncorretoException(string cep) : base($"O CEP {cep} está incorreto ou não foi encontrado")
        {
        }
    }
}