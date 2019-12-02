//using Newtonsoft.Json;
//using OmniCore.Repository.Enums;
//using OmniCore.Model.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using OmniCore.Model.Enumerations;
//
//namespace OmniCore.Eros
//{
//    public class ErosRequestParameters
//    {
//        public static ErosRequestParameters FromJson(RequestType requestType, string parametersJson)
//        {
//            switch(requestType)
//            {
//                case RequestType.Pair:
//                    return JsonConvert.DeserializeObject<ErosRequestParametersPair>(parametersJson);
//                default:
//                    return null;
//            }
//        }
//
//        public string ToJson()
//        {
//            return JsonConvert.SerializeObject(this);
//        }
//    }
//
//    public class ErosRequestParametersPair : ErosRequestParameters
//    {
//        public uint RadioAddress { get; set; }
//    }
//}
