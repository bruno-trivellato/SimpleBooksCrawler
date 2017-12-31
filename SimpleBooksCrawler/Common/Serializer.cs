using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SimpleBooksCrawler.Common
{
    public static class Serializer
    {

        public static void SerializeInJson<T>(T genericObject, string fileFullPath)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = File.OpenWrite(fileFullPath))
            {
                serializer.WriteObject(stream, genericObject);
            }
        }

        public static T DeserializeInJson<T>(string fileFullPath)
        {
            var jsonSerializer = new DataContractJsonSerializer(typeof(T));

            try
            {
                using (Stream myStream = File.OpenRead(fileFullPath))
                {
                    StreamReader reader = new StreamReader(myStream);
                    //var test = await reader.ReadToEndAsync();

                    return (T)jsonSerializer.ReadObject(myStream);
                }
            }
            catch (SerializationException)
            {
                // bad format in file. It is not T.
                throw;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
