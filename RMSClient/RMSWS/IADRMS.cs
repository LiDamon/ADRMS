using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace RMSWS
{
    [ServiceContract]
    public interface IRMSWS
    {

        [OperationContract]
        string Protect(string ownerEmailAddress, string filePath, string templateName, string listOfRights);

        [OperationContract]
        string Unprotect(string filePath);

    }
}
