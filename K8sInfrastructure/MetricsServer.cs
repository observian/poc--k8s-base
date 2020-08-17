using System.Collections.Generic;
using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.ApiRegistration.V1;
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
	public class MetricsServer
	{
		public MetricsServer(Provider provider)
		{
			var clusterRole = new ClusterRole("metrics-server-cluster-role", new ClusterRoleArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "system:aggregated-metrics-reader",
					Labels = new InputMap<string>
					{
						{"rbac.authorization.k8s.io/aggregate-to-view", "true"},
						{"rbac.authorization.k8s.io/aggregate-to-edit", "true"},
						{"rbac.authorization.k8s.io/aggregate-to-admin", "true"}
					}
				},
				Rules = new InputList<PolicyRuleArgs>
				{
					new PolicyRuleArgs
					{
						ApiGroups = new InputList<string> {"metrics.k8s.io"},
						Resources = new InputList<string> {"pods", "nodes"},
						Verbs = new InputList<string> {"get", "list", "watch"}
					}
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var metricsClusterRoleBinding = new ClusterRoleBinding("metrics-cluster-role-binding",
				new ClusterRoleBindingArgs
				{
					Metadata = new ObjectMetaArgs
					{
						Name = "metrics-server:system:auth-delegator"
					},
					RoleRef = new RoleRefArgs
					{
						ApiGroup = "rbac.authorization.k8s.io",
						Kind = "ClusterRole",
						Name = "system:auth-delegator"
					},
					Subjects = new List<SubjectArgs>
					{
						new SubjectArgs
						{
							Kind = "ServiceAccount",
							Name = "metrics-server",
							Namespace = "kube-system"
						}
					}
				}, new CustomResourceOptions
				{
					Provider = provider
				});
			var metricsRoleBinding = new RoleBinding("metricsRoleBinding", new RoleBindingArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "metrics-server-auth-reader",
					Namespace = "kube-system"
				},
				RoleRef = new RoleRefArgs
				{
					ApiGroup = "rbac.authorization.k8s.io",
					Kind = "Role",
					Name = "extension-apiserver-authentication-reader"
				},
				Subjects = new InputList<SubjectArgs>
				{
					new SubjectArgs
					{
						Kind = "ServiceAccount",
						Name = "metrics-server",
						Namespace = "kube-system"
					}
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var service = new APIService("v1beta1.metrics.k8s.io", new APIServiceArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "v1beta1.metrics.k8s.io",
					Namespace = "kube-system"
				},
				Spec = new APIServiceSpecArgs
				{
					Service = new ServiceReferenceArgs
					{
						Name = "metrics-server",
						Namespace = "kube-system"
					},
					Group = "metrics.k8s.io",
					Version = "v1beta1",
					InsecureSkipTLSVerify = true,
					GroupPriorityMinimum = 100,
					VersionPriority = 100
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var metricsServiceAccount = new ServiceAccount("metrics-service-account", new ServiceAccountArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "metrics-server",
					Namespace = "kube-system"
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var metricsServerDeployment = new Pulumi.Kubernetes.Apps.V1.Deployment("metrics-server-deployment",
				new DeploymentArgs
				{
					Metadata = new ObjectMetaArgs
					{
						Name = "metrics-server",
						Namespace = "kube-system",
						Labels = new InputMap<string>
						{
							{"k8s-app", "metrics-server"}
						}
					},
					Spec = new DeploymentSpecArgs
					{
						Selector = new LabelSelectorArgs
						{
							MatchLabels = new InputMap<string>()
							{
								{"k8s-app", "metrics-server"}
							}
						},
						Template = new PodTemplateSpecArgs
						{
							Metadata = new ObjectMetaArgs
							{
								Name = "metrics-server",
								Labels = new InputMap<string>
								{
									{"k8s-app", "metrics-server"}
								}
							},
							Spec = new PodSpecArgs
							{
								ServiceAccountName = "metrics-server",
								Volumes = new List<VolumeArgs>
								{
									new VolumeArgs
									{
										Name = "tmp-dir",
										EmptyDir = { }
									}
								},
								Containers = new List<ContainerArgs>
								{
									new ContainerArgs
									{
										Name = "metrics-server",
										Image = "k8s.gcr.io/metrics-server-amd64:v0.3.6",
										ImagePullPolicy = "IfNotPresent",
										Command = new InputList<string>
										{
											"/metrics-server",
											"--metric-resolution=30s",
											"--kubelet-insecure-tls",
											"--kubelet-preferred-address-types=InternalIP,Hostname,InternalDNS,ExternalDNS,ExternalIP",
											"--cert-dir=/tmp",
											"--secure-port=4443"
										},
										Ports = new List<ContainerPortArgs>
										{
											new ContainerPortArgs
											{
												Name = "main-port",
												ContainerPortValue = 4443,
												Protocol = "TCP"
											}
										},
										SecurityContext = new SecurityContextArgs
										{
											ReadOnlyRootFilesystem = true,
											RunAsNonRoot = true,
											RunAsUser = 1000
										},
										VolumeMounts = new InputList<VolumeMountArgs>
										{
											new VolumeMountArgs
											{
												Name = "tmp-dir",
												MountPath = "/tmp"
											}
										}
									}
								},
								NodeSelector = new InputMap<string>()
								{
									{"kubernetes.io/os", "linux"},
									{"kubernetes.io/arch", "amd64"}
								}
							}
						}
					}
				}, new CustomResourceOptions
				{
					Provider = provider
				});
			var metricsServerService = new Pulumi.Kubernetes.Core.V1.Service("metrics-server-service", new ServiceArgs
			{
				ApiVersion = "v1",
				Kind = "Service",
				Metadata = new ObjectMetaArgs
				{
					Name = "metrics-server",
					Namespace = "kube-system",
					Labels = new InputMap<string>
					{
						{"kubernetes.io/name", "Metrics-server"},
						{"kubernetes.io/cluster-service", "true"}
					}
				},
				Spec = new ServiceSpecArgs
				{
					Selector = new InputMap<string>
					{
						{"k8s-app", "metrics-server"}
					},
					Ports = new InputList<ServicePortArgs>
					{
						new ServicePortArgs
						{
							Name = "https",
							Port = 443,
							Protocol = "TCP",
							TargetPort = "main-port"
						},
					}
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var metricsClusterRole = new ClusterRole("metrics-server-cluster-role-2", new ClusterRoleArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "system:metrics-server"
				},
				Rules = new InputList<PolicyRuleArgs>
				{
					new PolicyRuleArgs
					{
						ApiGroups = new InputList<string> {""},
						Resources = new InputList<string>
						{
							"pods",
							"nodes",
							"nodes/stats",
							"namespaces",
							"configmaps"
						},
						Verbs = new InputList<string>
						{
							"get",
							"list",
							"watch"
						}
					}
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});
			var metricsServerClusterRoleBinding = new ClusterRoleBinding("metrics-server-cluster-role-binding",
				new ClusterRoleBindingArgs
				{
					Metadata = new ObjectMetaArgs
					{
						Name = "system:metrics-server"
					},
					RoleRef = new RoleRefArgs
					{
						ApiGroup = "rbac.authorization.k8s.io",
						Kind = "ClusterRole",
						Name = "system:metrics-server"
					},
					Subjects = new InputList<SubjectArgs>
					{
						new SubjectArgs
						{
							Kind = "ServiceAccount",
							Name = "metrics-server",
							Namespace = "kube-system"
						}
					}
				}, new CustomResourceOptions
				{
					Provider = provider
				});
		}
	}
}