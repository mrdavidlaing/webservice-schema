using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using MetadataProcessor.DTOTestAssembly;

namespace TestAssembly.Service
{
        /// <summary>
        /// This service contains DELETE and PUT methods
        /// </summary>
        ///<smd />
        [ServiceContract]
        public interface IServiceWithDELETEandPUTMethods
        {
            ///<summary>
            /// We don't currently support DELETE methods since most HTTP clients only support GET and POST well
            ///</summary>
            ///<returns></returns>
            ///<smd />
            [OperationContract]
            [WebInvoke(Method="DELETE",
                UriTemplate = "/IServiceWithDELETEandPUTMethods/delete",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 DeleteEndpoint();

            ///<summary>
            /// We don't currently support PUT methods since most HTTP clients only support GET and POST well
            ///</summary>
            ///<returns></returns>
            ///<smd />
            [OperationContract]
            [WebInvoke(Method = "PUT",
                UriTemplate = "/IServiceWithDELETEandPUTMethods/put",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 PutEndpoint();
        }

}
