using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;


namespace EstudoRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MensageriaController : Controller
    {
        private readonly IConfiguration _configuration;       
        private readonly IConnectionFactory _connectionFactory;
        private const string NOME_FILA ="filadeteste";

        public MensageriaController(IConfiguration configuration, IConnectionFactory connectionFactory)
        {
            _configuration = configuration;           
            _connectionFactory = connectionFactory;
            _connectionFactory.Uri = new Uri(_configuration.GetSection("CLOUDAMQP_URL").Value);
        }    

        [HttpGet]
        public JsonResult Get()
        {
            using (var conn = _connectionFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {
                channel.QueueDeclare(NOME_FILA, true, false, false, null);
                var queueName = NOME_FILA;
                var data = channel.BasicGet(queueName, false);

                if (data == null) return Json(null);

                var message = Encoding.UTF8.GetString(data.Body);
                
                channel.BasicAck(data.DeliveryTag, false);
                return Json(message);
            }
        }

        [HttpPost]
        public ActionResult Post(string mensagem)
        {                       
            using (var conn = _connectionFactory.CreateConnection())
            using (var channel = conn.CreateModel())
            {                                             
                var data = Encoding.UTF8.GetBytes(mensagem);                
                var queueName = NOME_FILA;
                bool durable = true;
                bool exclusive = false;
                bool autoDelete = false;

                channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);  
                
                var exchangeName = "";
                var routingKey = NOME_FILA;

                channel.BasicPublish(exchangeName, routingKey, null, data);
            }

            return new EmptyResult();
        }
    }
}
