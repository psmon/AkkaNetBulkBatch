using BulkBatchApp.Config;
using BulkBatchApp.Entity;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BulkBatchApp.Adapters
{
    public interface IElasticEngine
    {
        IElasticClient GetClientForTestEvent();
    }

    public class ElasticEngine : IElasticEngine
    {
        private IElasticClient clientForTestEvent { get; set; }

        public ElasticEngine(AppSettings appSettings)
        {
            var defaultSettings = new ConnectionSettings(new Uri(appSettings.ElkConnection))
                .DefaultIndex(appSettings.ElkIndex)
                .DefaultMappingFor<TestEvent>(m => m                    
                    .IdProperty(p => p.id)
                );

            clientForTestEvent = new ElasticClient(defaultSettings);

        }

        public IElasticClient GetClientForTestEvent()
        {
            return clientForTestEvent;
        }

        public async Task BulkInsertOrUpdateToEventTable(List<TestEvent> testEvents)
        {
            var descriptorProductPerPage = new BulkDescriptor();

            foreach(var testEvent in testEvents)
            {
                descriptorProductPerPage.Index<TestEvent>(op => op
                .Document(testEvent));
            }

            var result = await GetClientForTestEvent().BulkAsync(descriptorProductPerPage);
            if (!result.ApiCall.Success)
            {
                throw new Exception($"Failed Elk-Bulk Insert : messge {result.ServerError}");
            }
        }
    }
}
