using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using K8sDemo;
using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Config = Pulumi.Config;

public class K8sStack : Stack
{
    public K8sStack()
    {
        var config = new Config();
        
        var k8sStack = new StackReference(config.Require("stackreference"));
        var kubeConfig = k8sStack.Outputs.Apply(x =>
        {

            var config = x.GetValueOrDefault("KubeConfig").ToString();
            
            var provider = new Provider("kubernetes", new ProviderArgs
            {
                
                Cluster = x.GetValueOrDefault("ClusterName").ToString(),
                KubeConfig = config
            });
            var harnessDelegate = new HarnessWorkload(provider);
        
            var gremlinNamespace = new Namespace("gremlinNamespace", new NamespaceArgs
            {
                Metadata = new ObjectMetaArgs
                {
                    Name = "gremlin"
                }
            }, new CustomResourceOptions
            {
                Provider = provider
            });
            var testDeployment = new DemoApplication(provider);
            
            Console.WriteLine(config);
            return config?.ToString();
        });
        Console.WriteLine(kubeConfig.ToString());
    }
}