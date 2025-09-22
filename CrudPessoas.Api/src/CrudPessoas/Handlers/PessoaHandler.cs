using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using CrudPessoas.Models;
using CrudPessoas.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace CrudPessoas.Handlers;

public class PessoaHandler
{
    private readonly PessoaService _service;

    public PessoaHandler()
    {
        _service = new PessoaService();
    }

    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        switch (request.HttpMethod.ToUpper())
        {
            case "GET":
                if (request.Path == "/pessoas")
                    return await GetAllAsync(request, context);

                if (request.PathParameters != null && request.PathParameters.ContainsKey("id"))
                    return await GetByIdAsync(request, context);

                break;

            case "POST":
                if (request.Path == "/pessoas")
                    return await CreateAsync(request, context);
                break;

            case "PUT":
                if (request.PathParameters != null && request.PathParameters.ContainsKey("id"))
                    return await UpdateAsync(request, context);
                break;

            case "DELETE":
                if (request.PathParameters != null && request.PathParameters.ContainsKey("id"))
                    return await DeleteAsync(request, context);
                break;
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 404,
            Body = "Endpoint não encontrado"
        };
    }

    private async Task<APIGatewayProxyResponse> GetAllAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var pessoas = await _service.GetAllAsync();
        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(pessoas)
        };
    }

    private async Task<APIGatewayProxyResponse> GetByIdAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.PathParameters.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Id inválido" };

        var pessoa = await _service.GetByIdAsync(id);
        if (pessoa == null)
            return new APIGatewayProxyResponse { StatusCode = 404, Body = "Pessoa não encontrada" };

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(pessoa)
        };
    }

    private async Task<APIGatewayProxyResponse> CreateAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var pessoa = JsonSerializer.Deserialize<Pessoa>(request.Body ?? "");
        try
        {
            var pessoaCriada = await _service.CreateAsync(pessoa);
            return new APIGatewayProxyResponse
            {
                StatusCode = 201,
                Body = JsonSerializer.Serialize(pessoaCriada)
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = ex.Message
            };
        }
    }

    private async Task<APIGatewayProxyResponse> UpdateAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var pessoa = JsonSerializer.Deserialize<Pessoa>(request.Body ?? "");
        try
        {
            var pessoaAtualizada = await _service.UpdateAsync(pessoa);
            if (pessoaAtualizada == null)
                return new APIGatewayProxyResponse { StatusCode = 404, Body = "Pessoa não encontrada" };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(pessoaAtualizada)
            };
        }
        catch (Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = ex.Message
            };
        }
    }

    private async Task<APIGatewayProxyResponse> DeleteAsync(APIGatewayProxyRequest request, ILambdaContext context)
    {
        if (!request.PathParameters.TryGetValue("id", out var id) || string.IsNullOrWhiteSpace(id))
            return new APIGatewayProxyResponse { StatusCode = 400, Body = "Id inválido" };

        var deleted = await _service.DeleteAsync(id);
        return new APIGatewayProxyResponse
        {
            StatusCode = deleted ? 204 : 404,
            Body = deleted ? "" : "Pessoa não encontrada"
        };
    }
}
