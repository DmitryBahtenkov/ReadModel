using Nest;
using ReadModel.Models;

namespace ReadModel.Elastic;

public class ElasticRepository
{
    // наименование индекса в elastic
    private const string IndexName = "employee-index";
    // объект клиента к elasticsearch, через который будем выполнять все запросы
    private readonly ElasticClient _elasticClient;

    public ElasticRepository(ElasticClient elasticClient)
    {
        _elasticClient = elasticClient;
    }

    public async Task IndexDocument(Employee employee)
    {
        await _elasticClient.IndexAsync(employee, request => request.Index(IndexName));
    }

    public async IAsyncEnumerable<Employee> Search(SearchDescriptor<Employee> searchDescriptor)
    {
        var response = await _elasticClient.SearchAsync<Employee>(searchDescriptor);
        foreach (var hit in response.Hits)
        {
            yield return hit.Source;
        }
    }

    public async Task UpdateDocument(Employee employee)
    {
        await _elasticClient.UpdateAsync(DocumentPath<Employee>.Id(employee), x => x.Upsert(employee));
    }

    public async Task DeleteDocument(Employee employee)
    {
        await _elasticClient.DeleteAsync(DocumentPath<Employee>.Id(employee));
    }
}