using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace ValidationTest.Model
{
    [DataContract]
    public class RequestMessage
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Time Stamp  is required")]
        [DataMember(Name = "ts")]
        public string Ts { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Sender is required")]
        [DataMember(Name = "sender")]
        public string Sender { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Message must be entered")]
        [MustHaveOneElementAttribute(ErrorMessage = "Message must have at least one element")]
        [DataMember(Name = "message")]
        public JObject Message { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "sent_from_ip must be entered")]
        [DataMember(Name = "sent-from-ip")]
        public string sent_from_ip { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "priority must be entered")]
        [DataMember(Name = "priority")]
        public int Priority { get; set; }
    }
    [DataContract]
    public class JsonObject
    {

    }
    public class MustHaveOneElementAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var jObject = value as JObject;
            if (jObject != null)
            {
                return jObject.Children().Count() > 0;
            }
            return false;
        }
    }
    public class ValidJsonMessageElementAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
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
            JObject user = JObject.Parse((string)value);

            bool isValid = user.IsValid(schema);
            return isValid;
        }
    }
}
