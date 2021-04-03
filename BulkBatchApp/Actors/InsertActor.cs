using Akka.Actor;
using AkkaDotModule.ActorUtils;
using AkkaDotModule.Models;
using BulkBatchApp.Entity;
using Microsoft.Extensions.DependencyInjection;

namespace BulkBatchApp.Actors
{
    public class InsertActor : ReceiveActor
    {
        private readonly IActorRef _bulkWriterActor;
        private readonly IActorRef _batchActor;

        //벌크옵션, bulkSec 동안모아두고 처리하거나, bulkCount만큼찼을때
        int bulkSec = 3;
        int bulkCount = 1000;
        int eventCount = 0;

        public InsertActor(IServiceScopeFactory scopeFactory)
        {
            _bulkWriterActor = Context.ActorOf(Props.Create(() => new BulkWriterActor(scopeFactory)));
            _batchActor = Context.ActorOf(Props.Create(() => new BatchActor(bulkSec)));
            _batchActor.Tell(new SetTarget(_bulkWriterActor));  //배치처리기 연결( 롤설정된 배치만큼 처리를 요청)
            ReceiveAsync<InsertOrUpdateTestEvent>(async insertOrUpdateTestEvent =>
            {
                _batchActor.Tell(new Queue(insertOrUpdateTestEvent));
                eventCount++;
                if (eventCount > bulkCount)
                {
                    eventCount = 0;
                    //버퍼오버플로우 방지를 위해, 지금까지 받은 데이터 처리
                    _batchActor.Tell(new Flush());
                }
            });
        }
    }
}
