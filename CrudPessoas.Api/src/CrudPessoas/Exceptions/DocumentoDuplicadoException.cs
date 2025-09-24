namespace CrudPessoas.Exceptions
{
    public class DocumentoDuplicadoException : Exception
    {
        public DocumentoDuplicadoException(string documento) : base($"Documento {documento} já cadastrado em nossa base de dados")
        {
        }
    }
}