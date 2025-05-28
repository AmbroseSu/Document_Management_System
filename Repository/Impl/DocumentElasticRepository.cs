using BusinessObject.Option;
using Elastic.Clients.Elasticsearch;

namespace Repository.Impl;

public class DocumentElasticRepository : IDocumentElasticRepository
{
    
    private readonly ElasticsearchClient _client;

    public DocumentElasticRepository(ElasticsearchClient client)
    {
        _client = client;
    }

    private const string IndexName = "documents";
    
    public async Task<bool> AddAsync(DocumentElastic document)
    {
        var response = await _client.IndexAsync(document, idx => idx
            .Index(IndexName)
            .Id(document.DocumentId.ToString()));

        return response.IsSuccess();
    }

    public async Task<DocumentElastic> GetByIdAsync(Guid id)
    {
        var response = await _client.GetAsync<DocumentElastic>(IndexName, id);
        return response.IsSuccess() && response.Found ? response.Source : null;
    }

    public async Task<IEnumerable<DocumentElastic>> SearchAsync(string query)
    {
        var response = await _client.SearchAsync<DocumentElastic>(s => s
            .Index(IndexName)
            .Query(q => q
                .Match(m => m
                    .Field("documentname")
                    .Query(query)
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            ));

        return response.IsSuccess() ? response.Hits.Select(h => h.Source) : Enumerable.Empty<DocumentElastic>();
    }
    public async Task<bool> UpdateAsync(DocumentElastic document)
    {
        var response = await _client.UpdateAsync<DocumentElastic, DocumentElastic>(IndexName, document.DocumentId.ToString(), u => u
            .Doc(document));

        return response.IsSuccess();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _client.DeleteAsync(IndexName, id);
        return response.IsSuccess();
    }
}