namespace CrudPessoas.Exceptions
{
    public class DocumentoInvalidoException : Exception
    {
        public DocumentoInvalidoException(string documento) : base($"Documento {documento} inválido")
        {
        }
    }
}