using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using ValidationTest.Model;
using static ValidationTest.Filter.ValidationFilter;

namespace ValidationTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMessageController : ControllerBase
    {
        // GET: api/TestMessage
        [HttpGet]
        public string Get()
        {
            return "Unity Test Case V 0.9 By Alireza Abdelahi. Powered by ASP.NET Core";
        }

        // POST: api/TestMessage
        [HttpPost]
        //[ServiceFilter(typeof(ValidateModelAttribute))]
        public IActionResult Post([FromBody] JObject message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (!IsValid(message))
            {
                return BadRequest();
            }
            var config = new Dictionary<string, object>{
                { "bootstrap.servers", "localhost:9092" }
             };
            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
            
                producer.ProduceAsync("hello-topic", null, message.ToString());

                producer.Flush(100);
            }           
            return Ok(true);
        }
        public bool IsValid(JObject message)
        {
            #region Schema definition
            JSchema schema = JSchema.Parse(@"{
                '$id': 'http://example.com/example.json',
                    'type': 'object',
                        'additionalProperties': false,
                            'required': [
                                'ts',
                                'sender',
                                'message'
                            ],
                                'definitions': { },
                '$schema': 'http://json-schema.org/draft-07/schema#',
                    'properties': {
                    'ts': {
                        '$id': '/properties/ts',
                            'type': 'string',
                                'title': 'The Ts Schema ',
                                    'default': '',
                                        'pattern': '^[0-9]{1,10}$',
                                            'examples': [
                                                '1530228282'
                                            ]
                    },
                    'sender': {
                        '$id': '/properties/sender',
                            'type': 'string',
                                'title': 'The Sender Schema ',
                                    'default': '',
                                        'examples': [
                                            'testy-test-service'
                                        ]
                    },
                    'message': {
                        '$id': '/properties/message',
                            'type': 'object',
                                'minProperties': 1,
                                    'properties': { },
                        'additionalProperties': {
                            'type': 'string',
                                'minItems': 1,
                                    'description': 'string values'
                        }
                    },
                    'sent-from-ip': {
                        '$id': '/properties/sent-from-ip',
                            'type': 'string',
                                'format': 'ipv4',
                                    'default': '',
                                        'examples': [
                                            '1.2.3.4'
                                        ]
                    },
                    'priority': {
                        '$id': '/properties/priority',
                            'type': 'integer',
                                'title': 'The Priority Schema ',
                                    'default': 0,
                                        'examples': [
                                            2
                                        ]
                    }
                }
            }");
            #endregion
            //JObject message = JObject.Parse(value);
            IList<string> validationErrors;
            bool isValid = message.IsValid(schema, out validationErrors);
            return isValid;
        }
    }
}
