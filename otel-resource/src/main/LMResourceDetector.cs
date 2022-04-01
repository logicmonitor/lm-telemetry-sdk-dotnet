using System;
using System.Collections.Generic;
using System.Text;
using ec2;
using LMTelemetrySDK.azure;

namespace LMResourceDetector
{
    public class ResourceDetector
    {
        private static Dictionary<string, object> ResourceList;
        private static readonly string resource_attributre_env = "OTEL_RESOURCE_ATTRIBUTES";

        public static Dictionary<string, object> Detect()
        {
            Console.WriteLine("Detecting Resource");
            ResourceList = new Dictionary<string, object>();
            Dictionary<string, object> _resourceList = new Dictionary<string, object>();
            //check if EC2 resource.
            _resourceList=LMEc2Resource.get();
            if (_resourceList == null)
            {
                _resourceList = LmAzurevmResource.get();
            }
            if (_resourceList != null)
            {
                foreach (var item in _resourceList)
                {
                    ResourceList.Add(item.Key, item.Value);
                }
            }
            var _setResource = Environment.GetEnvironmentVariable("SET_RESOURCE_ATTRIBUTE") ?? "0";
            if (_setResource.Equals("1"))
            {
                PopulateResourceEnvVariable();
            }
            return ResourceList;
        }

        private static void PopulateResourceEnvVariable()
        {
            string otel_res_attributes = Environment.GetEnvironmentVariable(resource_attributre_env) ?? null;
            StringBuilder sb = new StringBuilder();
            if (otel_res_attributes != null)
            {
                sb.Append(otel_res_attributes);
                sb.Append(",");
            }
            if (ResourceList != null)
            {
                foreach (var item in ResourceList)
                {
                    sb.Append(item.Key);
                    sb.Append("=");
                    sb.Append(item.Value);
                    sb.Append(",");
                }
            }
            if (sb.Length > 0)
            {
                otel_res_attributes = sb.ToString().Substring(0, sb.Length - 1);
                Console.WriteLine("Attributes detected:"+otel_res_attributes);

                Environment.SetEnvironmentVariable(resource_attributre_env, otel_res_attributes);
            }

        }
    }
}
