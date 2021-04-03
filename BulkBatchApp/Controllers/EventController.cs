using Akka.Actor;
using AkkaDotModule.Config;
using BulkBatchApp.Entity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace BulkBatchApp.Controllers
{
    [Route("api/[controller]")]
    public class EventController : Controller
    {
        private readonly IActorRef _insertActor;

        public EventController()
        {
            _insertActor = AkkaLoad.ActorSelect("InsertActor"); 
        }

        /// <summary>
        /// 사용자 이벤트를 발생시킨다.
        /// </summary>        
        /// <response code="200">성공</response>
        /// <response code="412">
        /// ....         
        /// </response>
        [HttpPost("EventRaise")]
        public async Task<string> EventRaise(            
            [FromBody] InsertOrUpdateTestEvent userEvent)
        {
            _insertActor.Tell(userEvent);
            var result = "ok";
            return result;
        }

        /// <summary>
        /// 사용자 이벤트를 다수 발생시킨다.
        /// </summary>        
        /// <param name="repeat">반복</param>        
        /// <response code="200">성공</response>
        /// <response code="412">
        /// ....         
        /// </response>
        [HttpPost("EventBulkRaise")]
        public async Task<string> EventBulkRaise(
            int repeat,
            [FromBody] InsertOrUpdateTestEvent userEvent)
        {
            for (int i= 0; i<repeat; i++)
            {
                InsertOrUpdateTestEvent bulkEvent = new InsertOrUpdateTestEvent()
                {
                    id = userEvent.id + i,                    
                    action_type = userEvent.action_type,
                    action_name = userEvent.action_name + "_" + i,
                    reg_dt = DateTime.Now
                };

                _insertActor.Tell(bulkEvent);
            }
            
            var result = "ok";
            return result;
        }

    }
}
