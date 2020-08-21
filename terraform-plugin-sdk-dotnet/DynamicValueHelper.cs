using System;
using System.Collections.Generic;
using System.Text;
using Tfplugin5;

namespace Matthid.TerraformSdk
{
    public class DynamicValueHelper
    {
        public static T Deserialize<T>(DynamicValue value)
        {
            if (value == null)
            {
                return default(T);
            }

            if (value.Msgpack != null && value.Msgpack.Length > 0)
            {
                return MessagePackHelper.Deserialize<T>(value.Msgpack);
            }
            
            if (value.Json != null && value.Json.Length > 0)
            {
                throw new NotImplementedException("Json not yet");
            }

            return default(T);
        }

        public static DynamicValue Serialize<T>(T value)
        {
            var d = new DynamicValue();
            d.Msgpack = MessagePackHelper.Serialize(value);
            return d;
        }
    }
}
