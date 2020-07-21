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
                        Labels = demoAppLabels,
                        Annotations = new InputMap<string>
                        {
                            {"iam.amazonaws.com/role", "arn:aws:iam::592516922976:role/aws_eks_pod_assume_role"}
                        }
                    },
                    Spec = new PodSpecArgs
                    {
                        Containers = new ContainerArgs
                        {
                            Name = "dotnetapp",
                            Image = "592516922976.dkr.ecr.us-west-2.amazonaws.com/ssm-tester:1.1",
                            Resources = new ResourceRequirementsArgs
                            {
                                Requests = new InputMap<string>
                                {
                                    { "cpu", "300m" }
                                }
                            },
                            Env = new List<EnvVarArgs>
                            {
                                new EnvVarArgs
                                {
                                    Name = "SSM_PARAMETER_PATH",
                                    Value = "/k8s-testing/development"
                                },
                                new EnvVarArgs
                                {
                                    Name = "AWS_REGION",
                                    Value = "us-west-2"
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
        
        //install the cluster autoscaler
        var clusterAutoScaler = new ClusterAutoScaler(config.Require("cluster-name"), "cluster-autoscaler");
        //install the metrics server
        var metricsServer = new MetricsServer();
        
        var kube2Iam = new Kube2Iam();

        //create an autoscaling service for the dotnet app
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
                            AverageUtilization = 20
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
                                Value = 20,
                                PeriodSeconds = 30
                            }
                        }
                    }
                }
                
            }
        });
        
    }
}