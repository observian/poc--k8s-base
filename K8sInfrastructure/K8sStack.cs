using System.Collections.Generic;
using K8sDemo;
using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

public class K8sStack : Stack
{
    public K8sStack()
    {
        var config = new Config();
        
        var harnessDelegate = new HarnessWorkload();
        
        //TODO: istio layers
        //TODO: application layers
        
        var gremlinNamespace = new Namespace("gremlinNamespace", new NamespaceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Name = "gremlin"
            }
        });
        
        //install the cluster autoscaler
        var clusterAutoScaler = new ClusterAutoScaler(config.Require("cluster-name"), "cluster-autoscaler");
        //install the metrics server
        var metricsServer = new MetricsServer();
        
        var kube2Iam = new Kube2Iam();
        
        var testDeployment = new DemoApplication();
        
        
        
        

    }
}