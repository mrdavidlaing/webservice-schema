using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using TestAssembly.DTO;

namespace TestAssembly.Service
{
        /// <summary>
        /// This service contains methods with different endpoints / targets
        /// </summary>
        ///<smd />
        [ServiceContract]
        public interface ISimpleService
        {
            ///<summary>
            /// A simple service to get a list of items
            ///</summary>
            ///<returns></returns>
            ///<smd />
            [OperationContract]
            [WebInvoke(Method="GET",
                UriTemplate = "/",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            IList<SampleDto> GetAll();

            ///<summary>
            /// A simple service to get an item detail
            ///</summary>
            ///<returns></returns>
            ///<smd />
            [OperationContract]
            [WebInvoke(Method = "GET",
                UriTemplate = "/detail",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            SampleDto GetDetail();

            ///<summary>
            /// Service with overridden endpoint and uriTemplate
            ///</summary>
            ///<returns></returns>
            ///<smd endpoint="/endpoint/from/smd" uriTemplate="/uriTemplate/{from}/smd" />
            [OperationContract]
            [WebInvoke(Method = "GET",
                UriTemplate = "/uriTemplate/{from}/WebInvoke",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            SampleDto GetOverriddenExample();
        }

}
