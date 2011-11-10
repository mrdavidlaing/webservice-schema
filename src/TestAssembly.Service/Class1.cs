using System.ServiceModel;
using System.ServiceModel.Web;
using MetadataProcessor.DTOTestAssembly;

namespace TestAssembly.Service
{
        /// <summary>
        /// TODO: flesh this out with all the trimmings and edge cases 
        /// </summary>
        /// <smd/>
        [ServiceContract]
        public interface ITestService
        {
            ///<summary>
            /// CreateSession Method Description
            ///</summary>
            ///<returns></returns>
            /// <smd throttleScope="scope1"/>
            [OperationContract]
            [WebInvoke(Method = "POST",
                UriTemplate = "/",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 CreateSession();
           
            ///<summary>
            ///</summary>
            ///<param name="userName" minLength="6" maxLength="20" demoValue="CC735158">Username is case sensitive. May be set as a service parameter or as a request header.</param>
            ///<param name="session" format="guid" minLength="36" maxLength="36" demoValue="5998CBE8-3594-4232-A57E-09EC3A4E7AA8">The session token. May be set as a service parameter or as a request header.</param>
            ///<returns></returns>
            /// <smd 
            /// method="DeleteSession"
            /// />
            [OperationContract]
            [WebInvoke(Method = "POST", UriTemplate = "/deleteSession?userName={userName}&session={session}", ResponseFormat = WebMessageFormat.Json)]
            Class2 LogoutFromQueryString(string userName, string session);

            ///<summary>
            ///A simple WebInvoke(Method="GET") example
            ///</summary>
            ///<returns></returns>
            /// <smd />
            [OperationContract]
            [WebInvoke(Method = "GET",
                UriTemplate = "/WebInvokeMethodGet",
                ResponseFormat = WebMessageFormat.Json,
                RequestFormat = WebMessageFormat.Json)]
            Class1 WebInvokeMethodGet();
        }

}
