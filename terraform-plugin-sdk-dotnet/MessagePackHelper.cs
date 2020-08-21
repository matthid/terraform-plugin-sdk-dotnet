using Google.Protobuf;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Matthid.TerraformSdk
{
    public class MessagePackHelper
    {
        //public static object Deserialize(ByteString s)
        //{
        //    var bytes = s.ToByteArray();
        //    var obj = MessagePackSerializer.Deserialize( .Serialization.MessagePackSerializer.UnpackMessagePackObject(bytes);
        //    return obj;
        //}

        public static T Deserialize<T>(ByteString bs)
        {
            var bytes = bs.ToByteArray();
            return MessagePackSerializer.Deserialize<T>(bytes);
            //var s = MsgPack.Serialization.MessagePackSerializer.Get<T>();
            //return s.UnpackSingleObject(bytes);
        }


        public static ByteString Serialize<T>(T o)
        {
            var bytes = MessagePackSerializer.Serialize<T>(o);
            //var s = MsgPack.Serialization.MessagePackSerializer.Get<T>();
            //var bytes = s.PackSingleObject(o);
            return ByteString.CopyFrom(bytes);
        }
    }
}
