using System.ServiceModel;
using System.ServiceModel.Web;
using MetadataProcessor.DTOTestAssembly;

namespace TestAssembly.Service
{
        /// <summary>
        /// Only some of these service methods should be excluded
        /// </summary>
        /// <smd />
        [ServiceContract]
        public interface IPartiallyExcludedService
        {
            ///<summary>
            /// This service should be documented
            ///</summary>
            ///<returns></returns>
            ///<smd />
            [OperationContract]
            [WebGet(
                UriTemplate = "/PartiallyExcludedService/included",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 IncludedEndpoint();

            ///<summary>
            /// This service should not be documented
            ///</summary>
            ///<returns></returns>
            ///<smd exclude="true"/>
            [OperationContract]
            [WebGet(
                UriTemplate = "/PartiallyExcludedService/excluded",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 ExcludedEndpoint();
        }

}
