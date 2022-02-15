using System;
using System.Collections.Generic;
using System.Reflection;
using ec2;

namespace LMResourceDetector
{
    public class ResourceDetector
    {
        private static Dictionary<string, object> ResourceList;
        private static readonly string resource_attributre_env = "OTEL_RESOURCE_ATTRIBUTES";

        public static Dictionary<string, object> Detect()
        {
            var servicename = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? Assembly.GetEntryAssembly().GetName().Name;
            ResourceList = new Dictionary<string, object>();
            Dictionary<string, object> _resourceList = new Dictionary<string, object>();
            //check if EC2 resource.
            _resourceList=LMEc2Resource.get();
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
                Console.WriteLine("Resource Attribute Env Variable Set");
            }
            ResourceList.Add("service.name", servicename);
            return ResourceList;
        }

        private static void PopulateResourceEnvVariable()
        {
            string resource_attribute = "service=Test";
            if (ResourceList.Count != 0)
            {
                foreach (var item in ResourceList)
                {
                    resource_attribute += item.Key + "=" + item.Value + ",";
                }
                resource_attribute = resource_attribute.Substring(0, resource_attribute.Length - 1);
                Console.WriteLine(resource_attribute);
            }
            Environment.SetEnvironmentVariable(resource_attributre_env, resource_attribute);

        }
    }
}
