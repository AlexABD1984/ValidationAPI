using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValidationTest.Extension;

namespace ValidationTest.Filter
{
    public class ValidationFilter
    {
        public class ValidateModelAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext context)
            {
                if ((!context.ModelState.IsValid) || (!IsValidJson(context.HttpContext.Request).Result))
                {
                    context.Result = new BadRequestObjectResult(context.ModelState);
                }
                base.OnActionExecuting(context);
            }
            private async Task<bool> IsValidJson(HttpRequest request)
            {
                var json = await request.GetRawBodyStringAsync();
                return (IsValid(json));               
            }
            public bool IsValid(string value)
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
                JObject user = JObject.Parse(value);

                bool isValid = user.IsValid(schema);
                return isValid;
            }
        }
    }
}
