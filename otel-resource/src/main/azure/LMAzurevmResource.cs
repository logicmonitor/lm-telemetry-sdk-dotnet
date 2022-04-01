using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ThirdParty.Json.LitJson;

namespace LMTelemetrySDK.azure
{
    public class LmAzurevmResource
    {

        private static readonly string hostid = "host.id";
        private static readonly string cloudplatform = "cloud.platform";
        private static readonly string cloudplatform_value = "azure_vm";
        private static readonly string cloudprovider = "cloud.provider";
        private static readonly string cloudprovider_value = "azure";
        private static Dictionary<string, object> ResourceList = buildResource();

        public static Dictionary<string, object> get()
        {
            return ResourceList;
        }

        static async Task<HttpResponseMessage> PingAzureMetadata()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/metadata/instance?api-version=2017-08-01");
                request.Headers.Add("Metadata", "true");
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.SendAsync(request);
                return response;
            }
        }

        static async Task<string> getAzureVmID()
        {
            try
            {
                string hostid = null;
                var response = await PingAzureMetadata();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonMapper.ToObject(jsonString.ToString());
                    hostid = jsonDocument["compute"]["vmId"].ToString();
                    //Console.WriteLine("Host Id is :"+ hostid);
                    return hostid;
                }
                return null;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception :" + e);
                return null;

            }

        }

        static Dictionary<string, object> buildResource()
        {
            Dictionary<string, object> _resourceList = new Dictionary<string, object>();
            string AzureVmId = getAzureVmID().Result;
            if (AzureVmId != null)
            {

                _resourceList.Add(hostid, AzureVmId);
                _resourceList.Add(cloudplatform, cloudplatform_value);
                _resourceList.Add(cloudprovider, cloudprovider_value);
                return _resourceList;
                //return ResourceBuilder.CreateDefault().AddAttributes(ResourceList).Build();
            }
            else
            {
                //Console.WriteLine("Error Fetching EC2 details. Not An EC2 Resource");
                return null;
            }
        }
    }
}