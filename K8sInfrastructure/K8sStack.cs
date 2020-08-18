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
        var pulumiConfig = new Config();
        
        var k8sStack = new StackReference(pulumiConfig.Require("stackreference"));
        var kubeConfig = k8sStack.Outputs.Apply(x =>
        {

            var clusterId = x.GetValueOrDefault("ClusterName").ToString();
            var kubeConfig = x.GetValueOrDefault("KubeConfig").ToString();
            
            var provider = new Provider("kubernetes", new ProviderArgs
            {
                Cluster = clusterId,
                KubeConfig = kubeConfig
            });

            var harnessDelegate = new HarnessWorkload(provider);
            var gremlinInstall = new GremlinInstall(provider, clusterId, pulumiConfig);
            var testDeployment = new DemoApplication(provider);
            
            return kubeConfig?.ToString();
        });
    }
}