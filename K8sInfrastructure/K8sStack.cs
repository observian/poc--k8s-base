using K8sDemo;
using Pulumi;
using Pulumi.Kubernetes.Autoscaling.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using CrossVersionObjectReferenceArgs = Pulumi.Kubernetes.Types.Inputs.Autoscaling.V1.CrossVersionObjectReferenceArgs;
using HorizontalPodAutoscalerSpecArgs = Pulumi.Kubernetes.Types.Inputs.Autoscaling.V1.HorizontalPodAutoscalerSpecArgs;

public class K8sStack : Stack
{
    public K8sStack()
    {
        var demoAppLabels = new InputMap<string>
        {
            { "app", "dotnetapp" }
        };
			
        var demoNamespaceName = "demo";
        var k8sNamespace = new Namespace("demonamespace", new NamespaceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Name = demoNamespaceName
            }
        });
        
        var gremlinNamespace = new Namespace("gremlinNamespace", new NamespaceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Name = "gremlin"
            }
        });
        var deployment = new Pulumi.Kubernetes.Apps.V1.Deployment("dotnetapp-deployment", new DeploymentArgs
        {
            Kind = "Deployment",
            Metadata = new ObjectMetaArgs
            {
                Labels = demoAppLabels,
                Namespace = demoNamespaceName,
                Name = "dotnetapp-deployment"
            },
            Spec = new DeploymentSpecArgs
            {
                Selector = new LabelSelectorArgs
                {
                    MatchLabels = demoAppLabels
                },
                Replicas = 2,
                Template = new PodTemplateSpecArgs
                {
                    Metadata = new ObjectMetaArgs
                    {
                        Labels = demoAppLabels
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = new ContainerArgs
                        {
                            Name = "dotnetapp",
                            Image = "mcr.microsoft.com/dotnet/core/samples:aspnetapp",
                            Resources = new ResourceRequirementsArgs
                            {
                                Requests = new InputMap<string>
                                {
                                    { "cpu", "300m" }
                                }
                            }
                        }
                    }
                    
                }
            }
        });
        
        var dotnetService = new Service("dotnet-service", new ServiceArgs
        {
            Metadata = new ObjectMetaArgs
            {
                Labels = demoAppLabels,
                Namespace = demoNamespaceName
            },
            Spec = new ServiceSpecArgs
            {
                Ports = new ServicePortArgs
                {
                    Name = "http",
                    Port = 80,
                    Protocol = "TCP",
                    TargetPort = 80
                },
                Type = "LoadBalancer",
                Selector = demoAppLabels,
                
            }
        });
        
        var clusterAutoScaler = new ClusterAutoScaler("ob-eks-cluster-demo", "cluster-autoscaler");
        var metricsServer = new MetricsServer();

        var autoScalingService = new Pulumi.Kubernetes.Autoscaling.V2Beta2.HorizontalPodAutoscaler("hpa-example", new HorizontalPodAutoscalerArgs()
        {
            Metadata = new ObjectMetaArgs
            {
                Name = "dotnet-service-hpa",
                Namespace = "demo"
            },
            Spec = new Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2.HorizontalPodAutoscalerSpecArgs()
            {
                ScaleTargetRef = new Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2.CrossVersionObjectReferenceArgs
                {
                    Kind = "Deployment",
                    Name = "dotnetapp-deployment",
                    ApiVersion = "apps/v1"
                },
                MinReplicas = 1,
                MaxReplicas = 40,
                Metrics = new MetricSpecArgs
                {
                    Type = "Resource",
                    Resource = new ResourceMetricSourceArgs
                    {
                        Name = "cpu",
                        Target = new MetricTargetArgs
                        {
                            Type = "Utilization",
                            AverageUtilization = 50
                        }
                    }
                },
                Behavior = new HorizontalPodAutoscalerBehaviorArgs
                {
                    ScaleDown = new HPAScalingRulesArgs
                    {
                        Policies = new InputList<HPAScalingPolicyArgs>
                        {
                            new HPAScalingPolicyArgs
                            {
                                Type = "cpu",
                                Value = 10,
                                PeriodSeconds = 30
                            }
                        }
                    },
                    ScaleUp = new HPAScalingRulesArgs
                    {
                        Policies = new InputList<HPAScalingPolicyArgs>
                        {
                            new HPAScalingPolicyArgs
                            {
                                Type = "cpu",
                                Value = 50,
                                PeriodSeconds = 30
                            }
                        }
                    }
                }
                
            }
        });
        
    }
}