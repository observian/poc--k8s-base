using System.Collections.Generic;
using Pulumi;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace K8sDemo
{
	public class DemoApplication
	{
		public DemoApplication()
		{
			var config = new Config();

			var demoAppLabels = new InputMap<string>
			{
				{"app", "dotnetapp"}
			};

			var demoNamespaceName = "demo";
			var k8sNamespace = new Namespace("demonamespace", new NamespaceArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = demoNamespaceName
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
								Image = "observian/tcptest:1.2",
								Resources = new ResourceRequirementsArgs
								{
									Requests = new InputMap<string>
									{
										{"cpu", "300m"}
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
		}
	}
}