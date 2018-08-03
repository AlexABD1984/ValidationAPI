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
            return $"Unity Test Case By Alireza Abdelahi. Powered by ASP.NET Core - version 0.8.2.0 Hosted by {Environment.MachineName}";
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
            // validate Json object by schema
            if (!IsValidJson(message))
            {
                return BadRequest();
            }
            // Publish Message to Kafka Message Broker
            var r = "";
            var config = new Dictionary<string, object>{
                { "bootstrap.servers", "my-kafka:9092" }
             };
            using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
            {
                var dr = producer.ProduceAsync("test", null, message.ToString()).Result;
                r = $"Delivered to: {dr.TopicPartitionOffset} Offset={dr.Offset} TimeStamp={dr.Timestamp} Error={dr.Error}";
            }          
            
            return Ok(r);
        }
        /// <summary>
        /// This Methode validate json by Json Schema validator       
        /// </summary>
        /// <param name="message">Json object which need to validate</param>
        /// <returns>True | False : the result of validation</returns>
        public bool IsValidJson(JObject message)
        {
            //In real production, it is better to expose schema via url and cache it for better control over validation and update and performance
            #region Schema definition
            //Validation rules:  
            //● “ts” must be present and a valid Unix timestamp  
            //● “sender” must be present and a string  
            //● “message” must be present, a JSON object, and have at least one  field set  
            //● If present, “sent-from-ip” must be a valid IPv4 address  
            //● All fields not listed in the example above are invalid, and  should result in the message being rejected.  
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
            
            IList<string> validationErrors; // we can provide error list
            bool isValid = message.IsValid(schema, out validationErrors);   //Validate Json object by Schema Validation

            return isValid;
        }
    }
}
