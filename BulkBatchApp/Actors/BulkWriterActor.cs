using Akka.Actor;
using Akka.Event;
using AkkaDotModule.Models;
using BulkBatchApp.Adapters;
using BulkBatchApp.Entity;
using BulkBatchApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace BulkBatchApp.Actors
{
    public class BulkWriterActor : ReceiveActor
    {
        private readonly ILoggingAdapter logger = Context.GetLogger();
        private readonly IServiceScopeFactory _scopeFactory;

        public BulkWriterActor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            ReceiveAsync<object>(async message =>
            {
                //각각 저장하고 싶은 DB에따라 배치처리를 수행한다.
                if (message is Batch batchMessage)
                {
                    Batch batch = message as Batch;
                    //Type 검사 (BulkWriteActor는 동일한 Type의 컬렉션처리)
                    if (batch.Obj[0] is InsertOrUpdateTestEvent)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var eventRepository = scope.ServiceProvider.GetRequiredService<EventRepository>();
                            var elasitcEngine = scope.ServiceProvider.GetRequiredService<ElasticEngine>();

                            List<TestEvent> testEvents = new List<TestEvent>();
                            batch.Obj.ForEach(obj =>
                            {
                                var AddItem = obj as InsertOrUpdateTestEvent;
                                testEvents.Add(AddItem);
                            });

                            try
                            {
                                await elasitcEngine.BulkInsertOrUpdateToEventTable(testEvents);
                                await eventRepository.BulkInsertOrUpdateToEventTable(testEvents);
                            }
                            catch (Exception e)
                            {
                                logger.Error($"Failed BulkProductCateRelInsertOrUpdate: count:{testEvents.Count} e:{e.Message}");
                            }

                        }
                    }                
                }
            });
        }
    }
}
