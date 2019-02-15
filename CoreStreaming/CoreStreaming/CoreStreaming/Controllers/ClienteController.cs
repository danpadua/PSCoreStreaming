using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CoreStreaming.Enums;
using CoreStreaming.Models;
using CoreStreaming.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CoreStreaming.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private static readonly ConcurrentBag<StreamWriter> _clients;

        static ClienteController()
        {
            _clients = new ConcurrentBag<StreamWriter>();
        }

        [HttpPost]
        public IActionResult Post(Cliente cliente)
        {
            //Fazer o Insert
            EnviarEvento(cliente, EventoEnum.Insert);
            return Ok();
        }

        [HttpPut]
        public IActionResult Put(Cliente cliente)
        {
            //Fazer o Update
            EnviarEvento(cliente, EventoEnum.Update);
            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(Cliente cliente)
        {
            //Fazer o Update
            EnviarEvento(cliente, EventoEnum.Delete);
            return Ok();
        }

        private static async Task EnviarEvento(object dados, EventoEnum evento)
        {
            foreach (var client in _clients)
            {
                string jsonEvento = string.Format("{0}\n", JsonConvert.SerializeObject(new { dados, evento }));
                await client.WriteAsync(jsonEvento);
                await client.FlushAsync();
            }
        }

        [HttpGet]
        [Route("Streaming")]
        public IActionResult Stream()
        {
            return new PushStreamResult(OnStreamAvailable, "text/event-stream", HttpContext.RequestAborted);
        }

        private void OnStreamAvailable(Stream stream, CancellationToken requestAborted)
        {
            var wait = requestAborted.WaitHandle;
            var client = new StreamWriter(stream);
            _clients.Add(client);

            wait.WaitOne();

            _clients.TryTake(out StreamWriter ignore);
        }
    }
}