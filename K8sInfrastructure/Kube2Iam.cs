using System;
using System.Collections.Generic;
using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.ApiRegistration.V1;
using Pulumi.Kubernetes.Apps.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.ApiRegistration.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;
using ClusterRole = Pulumi.Kubernetes.Rbac.V1.ClusterRole;
using ClusterRoleArgs = Pulumi.Kubernetes.Types.Inputs.Rbac.V1.ClusterRoleArgs;
using ClusterRoleBinding = Pulumi.Kubernetes.Rbac.V1.ClusterRoleBinding;
using PolicyRuleArgs = Pulumi.Kubernetes.Types.Inputs.Rbac.V1.PolicyRuleArgs;
using RoleBinding = Pulumi.Kubernetes.Rbac.V1.RoleBinding;
using ServiceAccount = Pulumi.Kubernetes.Core.V1.ServiceAccount;


namespace K8sDemo
{
	public class Kube2Iam
	{
		public Kube2Iam(Provider provider)
		{
			
			
			
			var clusterRole = new ClusterRole("kube2Iam-clusterRole", new ClusterRoleArgs
			{
				ApiVersion = "rbac.authorization.k8s.io/v1",
				Kind = "ClusterRole",
				Metadata = new ObjectMetaArgs
				{
					Name = "kube2iam"
				},
				Rules = new InputList<PolicyRuleArgs>
				{
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> {string.Empty},
						Resources = new List<string>
						{
							"namespaces",
							"pods"
						},
						Verbs = new List<string>
						{
							"get",
							"watch",
							"list"
						}
					}
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});

			var clusterRoleBinding = new ClusterRoleBinding("kube2IamClusterRoleBinding", new ClusterRoleBindingArgs
			{
				ApiVersion = "rbac.authorization.k8s.io/v1",
				Kind = "ClusterRoleBinding",
				Metadata = new ObjectMetaArgs
				{
					Name = "kube2iam"
				},
				Subjects = new List<SubjectArgs>
				{
					new SubjectArgs
					{
						Kind = "ServiceAccount",
						Name = "kube2iam",
						Namespace = "default"
					}
				},
				RoleRef = new RoleRefArgs
				{
					Kind = "ClusterRole",
					Name = "kube2iam",
					ApiGroup = "rbac.authorization.k8s.io"
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});

			var kube2IamServiceAccount = new ServiceAccount("kube2iamserviceaccount", new ServiceAccountArgs
			{
				Kind = "ServiceAccount",
				ApiVersion = "v1",
				Metadata = new ObjectMetaArgs
				{
					Name = "kube2iam",
					Namespace = "default"
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});

			var kube2iamDaemonSet = new DaemonSet("kube2iamDaemonset", new DaemonSetArgs
			{
				Kind = "DaemonSet",
				Metadata = new ObjectMetaArgs
				{
					Name = "kube2iam",
					Namespace = "default",
					Labels = new InputMap<string>
					{
						{"app", "kube2iam"}
					}
				},
				Spec = new DaemonSetSpecArgs
				{
					Selector = new LabelSelectorArgs
					{
						MatchLabels = new InputMap<string>
						{
							{"app", "kube2iam"}
						}
					},
					UpdateStrategy = new DaemonSetUpdateStrategyArgs
					{
						Type = "RollingUpdate"
					},
					Template = new PodTemplateSpecArgs
					{
						Metadata = new ObjectMetaArgs
						{
							Labels = new InputMap<string>
							{
								{"app", "kube2iam"}
							}
						},
						Spec = new PodSpecArgs
						{
							ServiceAccountName = "kube2iam",
							HostNetwork = true,
							Containers = new List<ContainerArgs>
							{
								new ContainerArgs
								{
									Image = "jtblin/kube2iam:0.10.7",
									ImagePullPolicy = "Always",
									Name = "kube2iam",
									Args = new InputList<string>
									{
										"--auto-discover-base-arn",            
										"--auto-discover-default-role=true",            
										"--iptables=true",
										"--host-ip=$(HOST_IP)",
										"--node=$(NODE_NAME)",
										"--host-interface=eni+"
									},
									Env = new List<EnvVarArgs>
									{
										new EnvVarArgs
										{
											Name = "HOST_IP",
											ValueFrom = new EnvVarSourceArgs
											{
												FieldRef = new ObjectFieldSelectorArgs
												{
													FieldPath = "status.podIP"
												}
											}
										},
										new EnvVarArgs
										{
											Name = "NODE_NAME",
											ValueFrom = new EnvVarSourceArgs
											{
												FieldRef = new ObjectFieldSelectorArgs
												{
													FieldPath = "spec.nodeName"
												}
											}
										}
									},
									Ports = new InputList<ContainerPortArgs>
									{
										new ContainerPortArgs
										{
											ContainerPortValue = 8181,
											HostPort = 8181,
											Name = "http"
										}
									},
									SecurityContext = new SecurityContextArgs
									{
										Privileged = true
									}
								}
							}
						}
					}
				}
			});
		}
	}
}