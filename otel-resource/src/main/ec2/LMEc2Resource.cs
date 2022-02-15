using ThirdParty.Json.LitJson;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

namespace ec2
{
    public class LMEc2Resource{

        private static readonly string ARN = "aws.arn";
        private static readonly string CLOUD_PLATFORM = "cloud.platform";
        private static readonly string AWS_EC2_PLATFORM = "aws_ec2";
        private static readonly string REGION = "aws.region";
        private static readonly string HOSTNAME = "aws.hostname";
        private static readonly string CLOUD_PROVIDER = "cloud.provider";
        private static readonly string AVAILABILITY_ZONE = "cloud.availability_zone";
        private static readonly string AWS_PROVIDER = "aws";
        private static Dictionary<string, object> ResourceList = buildResource();
        public static Dictionary<string, object> get()
        {
            return ResourceList;
        }

        static string GetRegion(){

            string region = null;
            var identityDocument = Amazon.Util.EC2InstanceMetadata.IdentityDocument;
            if (!string.IsNullOrEmpty(identityDocument))
            {
                try
                {
                    var jsonDocument = JsonMapper.ToObject(identityDocument.ToString());
                    region = jsonDocument["region"].ToString();
                    if (region != null)
                        return region;
                }

                catch (Exception e)
                {
                    Console.WriteLine(e + "Error attempting to read region from instance metadata identity document");
                }
            }
            return region;
        }

        static string GetInstanceId(){
            return Amazon.Util.EC2InstanceMetadata.InstanceId;
        }

        static string GetAccountId()
        {
            string id = Amazon.Util.EC2InstanceMetadata.IdentityDocument;
            string[] data = id.Split(",");
            data = data[0].Split(":");
            return data[1];
        }

        static async Task<bool> IsEC2Instance() 
        {
            try
            {
                var response = await PingEC2();
                //Console.WriteLine("Response" + response + " " + response.StatusCode);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Exception :" + e);
                return false;

            }
            return true;


        }
        static async Task<HttpResponseMessage> PingEC2()
        {
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "http://169.254.169.254/");
                client.Timeout = TimeSpan.FromSeconds(2);
                var response = await client.SendAsync(request);
                return response;
            }
        }

        static string getARN(){
            string platform = "aws";
            string region = GetRegion();
            string accountId = GetAccountId();
            string instanceId = GetInstanceId();
            if (platform != null && region != null && accountId != null && instanceId != null)
                return "arn:" + platform + ":" + region + ":" + accountId + ":instance/" + instanceId;
            return null;
        }

        static string getHostName()
        {
            return Amazon.Util.EC2InstanceMetadata.Hostname;
        }
        static string getAvailabilityZone()
        {

            return Amazon.Util.EC2InstanceMetadata.AvailabilityZone;
        }

        static Dictionary<string,object> buildResource()
        {
            Dictionary<string, object> _resourceList = new Dictionary<string, object>();
            var isEc2instance = IsEC2Instance();

            if (isEc2instance.Result==true)
            {
                _resourceList.Add(ARN, getARN());
                _resourceList.Add(CLOUD_PLATFORM,AWS_EC2_PLATFORM);
                _resourceList.Add(REGION, GetRegion());
                _resourceList.Add(HOSTNAME, getHostName());
                _resourceList.Add(CLOUD_PROVIDER,AWS_PROVIDER);
                _resourceList.Add(AVAILABILITY_ZONE, getAvailabilityZone());
                return _resourceList;
                //return ResourceBuilder.CreateDefault().AddAttributes(ResourceList).Build();
            }
            else
            {
                //Console.WriteLine("Error Fetching EC2 details. Not An EC2 Resource");
                return null;
            }
        }

        private LMEc2Resource() { }
    }


}

