using Newtonsoft.Json;
using System;
using System.IO;

namespace FarmingTracker.DrfWebsocket
{
    public class CustomJsonConverter
    {
        public static DrfDrop Custom(string message)
        {
            //var message = "{\"kind\":\"data\",\"payload\":{\"character\":\"Ecksofa\",\"drop\":{\"items\":{},\"curr\":{\"1\":636626,\"2\":2083605,\"3\":132,\"7\":2112,\"15\":833,\"16\":4,\"18\":1063,\"19\":140,\"20\":6374,\"22\":7372,\"23\":348,\"24\":170,\"25\":1035,\"26\":63,\"27\":249,\"28\":1699,\"29\":114,\"30\":95,\"31\":2,\"32\":1992,\"33\":100,\"34\":2709,\"35\":499,\"36\":62,\"37\":205,\"38\":35,\"40\":62,\"41\":443,\"42\":259,\"43\":25,\"44\":51,\"45\":24,\"47\":100,\"49\":11,\"50\":13494,\"51\":307,\"54\":6,\"57\":81,\"58\":12055,\"59\":3010,\"60\":202,\"61\":32734,\"62\":39,\"65\":16,\"68\":80,\"69\":2553,\"70\":1},\"mf\":309,\"timestamp\":\"2022-08-01T13:59:34.660Z\"}}}\t";

            using (var reader = new JsonTextReader(new StringReader(message)))
            {
                var drfDrop = new DrfDrop();
                reader.Read(); // Token: StartObject
                reader.Read(); // Token: PropertyName, Value: kind
                reader.Read(); // Token: String, Value: data
                reader.Read(); // Token: PropertyName, Value: payload
                reader.Read(); // Token: StartObject
                reader.Read(); // Token: PropertyName, Value: character
                reader.Read(); // Token: String, Value: Ecksofa
                reader.Read(); // Token: PropertyName, Value: drop
                reader.Read(); // Token: StartObject
                reader.Read(); // Token: PropertyName, Value: items
                reader.Read(); // Token: StartObject

                //read items
                do
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                    var key = Convert.ToInt32(reader.Value);
                    reader.Read();
                    var value = Convert.ToInt32(reader.Value);
                    drfDrop.Items[key] = value;

                } while (true);

                reader.Read(); // Token: PropertyName, Value: curr
                reader.Read(); // Token: StartObject

                //read currencies
                do
                {
                    reader.Read();
                    if (reader.TokenType == JsonToken.EndObject)
                        break;
                    var key = Convert.ToInt32(reader.Value);
                    reader.Read();
                    var value = Convert.ToInt32(reader.Value);
                    drfDrop.Currencies[key] = value;
                } while (true);

                reader.Read(); // Token: PropertyName, Value: mf
                reader.Read(); // Token: Integer, Value: 309
                reader.Read(); // Token: PropertyName, Value: timestamp
                reader.Read(); // Token: Date, Value: 01.08.2022 13:59:34
                drfDrop.TimeStamp = Convert.ToDateTime(reader.Value);
                reader.Read(); // Token: EndObject
                reader.Read(); // Token: EndObject
                reader.Read(); // Token: EndObject
                return drfDrop;
            }
        }


        //while (reader.Read())
        //{
        //    if (reader.Value != null)
        //    {
        //        Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
        //    }
        //    else
        //    {
        //        Console.WriteLine("Token: {0}", reader.TokenType);
        //    }
        //}

        //while (reader.Read())
        //{
        //    if (reader.Value != null)
        //    {
        //        if (reader.TokenType == JsonToken.PropertyName)
        //            currentProperty = reader.Value.ToString();

        //        if (reader.TokenType == JsonToken.Integer && currentProperty == "items")
        //            drfDrop.Curr = ""


        //            if (reader.TokenType == JsonToken.Integer && currentProperty == "curr")
        //            drfDrop.Code = ;

        //        if (reader.TokenType == JsonToken.String && currentProperty == "timestamp")
        //            drfDrop.TimeStamp = reader.Value.ToString();
        //    }
        //}
    }
}
