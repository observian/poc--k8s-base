using System.Collections.Generic;
using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.Apps.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Rbac.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Apps.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Types.Inputs.Rbac.V1;
using ClusterRole = Pulumi.Kubernetes.Rbac.V1.ClusterRole;
using ClusterRoleArgs = Pulumi.Kubernetes.Types.Inputs.Rbac.V1.ClusterRoleArgs;
using PolicyRuleArgs = Pulumi.Kubernetes.Types.Inputs.Rbac.V1.PolicyRuleArgs;
using RoleArgs = Pulumi.Kubernetes.Types.Inputs.Rbac.V1.RoleArgs;

namespace K8sDemo
{
	public class ClusterAutoScaler
	{
		public ClusterAutoScaler(string clusterName, string autoScalerName)
		{
			var clusterAutoscalerLabels = new InputMap<string>
			{
				{"k8s-addon", "cluster-autoscaler.addons.k8s.io"},
				{"k8s-app", autoScalerName}
			};

			var autoscalerServiceAccount = new ServiceAccount("cluster-autoscaler-sa", new ServiceAccountArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Labels = clusterAutoscalerLabels,
					Name = autoScalerName,
					Namespace = "kube-system"
				}
			});

			var clusterAutoScalerClusterRole = new ClusterRole("cluster-autoscaler-role", new ClusterRoleArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Labels = clusterAutoscalerLabels,
					Name = autoScalerName
				},
				Rules = new List<PolicyRuleArgs>
				{
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"events",
							"endpoints"
						},
						Verbs = new List<string>
						{
							"create",
							"patch"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"pods/eviction"
						},
						Verbs = new List<string>
						{
							"create"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"pods/status"
						},
						Verbs = new List<string>
						{
							"update"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"endpoints"
						},
						ResourceNames = new List<string> {autoScalerName},
						Verbs = new List<string>
						{
							"get",
							"update"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"nodes"
						},
						Verbs = new List<string>
						{
							"watch",
							"list",
							"get",
							"update"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string> { "" },
						Resources = new List<string>
						{
							"pods",
							"services",
							"replicationcontrollers",
							"persistentvolumeclaims",
							"persistentvolumes"
						},
						Verbs = new List<string>
						{
							"watch",
							"list",
							"get"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"extensions"
						},
						Resources = new List<string>
						{
							"replicasets",
							"daemonsets"
						},
						Verbs = new List<string>
						{
							"watch",
							"list",
							"get"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"policy"
						},
						Resources = new List<string>
						{
							"poddisruptionbudgets"
						},
						Verbs = new List<string>
						{
							"watch",
							"list"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"apps"
						},
						Resources = new List<string>
						{
							"statefulsets",
							"replicasets",
							"daemonsets"
						},
						Verbs = new List<string>
						{
							"watch",
							"list",
							"get"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"storage.k8s.io"
						},
						Resources = new List<string>
						{
							"storageclasses",
							"csinodes"
						},
						Verbs = new List<string>
						{
							"watch",
							"list",
							"get"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"batch",
							"extensions"
						},
						Resources = new List<string>
						{
							"jobs"
						},
						Verbs = new List<string>
						{
							"get",
							"list",
							"watch",
							"patch"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"coordination.k8s.io",
						},
						Resources = new List<string>
						{
							"leases"
						},
						Verbs = new List<string>
						{
							"create"
						}
					},
					new PolicyRuleArgs
					{
						ApiGroups = new List<string>
						{
							"coordination.k8s.io"
						},
						ResourceNames = new List<string>
						{
							autoScalerName
						},
						Resources = new List<string>
						{
							"leases"
						},
						Verbs = new List<string>
						{
							"get",
							"update"
						}
					}
				}
			});

			var autoscalerRole = new Role("cluster-autoscaler-role", new RoleArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Labels = clusterAutoscalerLabels,
					Namespace = "kube-system",
					Name = autoScalerName
				},
				Rules = new List<PolicyRuleArgs>
				{
					new PolicyRuleArgs
					{
						ApiGroups = new InputList<string> { "" },
						Resources = new InputList<string>
						{
							"configmaps"
						},
						ResourceNames = new InputList<string>
						{
							"cluster-autoscaler-status",
							"cluster-autoscaler-priority-expander"
						},
						Verbs = new InputList<string>
						{
							"delete",
							"get",
							"update",
							"watch"
						}
					}
				}
			});

			var autoscalerClusterRoleBinding = new ClusterRoleBinding("autoscaler-clusterrole-binding",
				new ClusterRoleBindingArgs
				{
					Metadata = new ObjectMetaArgs
					{
						Name = autoScalerName,
						Labels = clusterAutoscalerLabels
					},
					RoleRef = new RoleRefArgs
					{
						ApiGroup = "rbac.authorization.k8s.io",
						Kind = "ClusterRole",
						Name = autoScalerName
					},
					Subjects = new InputList<SubjectArgs>
					{
						new SubjectArgs
						{
							Kind = "ServiceAccount",
							Name = autoScalerName,
							Namespace = "kube-system"
						}
					}
				});

			var autoscalerRoleBinding = new RoleBinding("autoscaler-role-binding", new RoleBindingArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = autoScalerName,
					Namespace = "kube-system",
					Labels = clusterAutoscalerLabels
				},
				RoleRef = new RoleRefArgs
				{
					Kind = "Role",
					Name = autoScalerName,
					ApiGroup = "rbac.authorization.k8s.io"
				},
				Subjects = new List<SubjectArgs>
				{
					new SubjectArgs
					{
						Kind = "ServiceAccount",
						Name = autoScalerName,
						Namespace = "kube-system"
					}
				}
			});

			var deploymentLabels = new InputMap<string>
			{
				{ "app", autoScalerName }
			};
			var autoscalerDeployment = new Pulumi.Kubernetes.Apps.V1.Deployment("autoscaler-deployment",
				new DeploymentArgs
				{
					Metadata = new ObjectMetaArgs
					{
						Name = autoScalerName,
						Namespace = "kube-system",
						Labels = deploymentLabels
					},
					Spec = new DeploymentSpecArgs
					{
						Replicas = 1,
						Selector = new LabelSelectorArgs
						{
							MatchLabels = deploymentLabels
						},
						Template = new PodTemplateSpecArgs
						{
							Metadata = new ObjectMetaArgs
							{
								Labels = deploymentLabels,
								Annotations = new InputMap<string>
								{
									{ "prometheus.io/scrape", "true" },
									{ "prometheus.io/port", "8085" }
								}
							},
							Spec = new PodSpecArgs
							{
								ServiceAccountName = autoScalerName,
								Containers = new InputList<ContainerArgs>
								{
									new ContainerArgs
									{
										Image = "k8s.gcr.io/cluster-autoscaler:v1.14.7",
										Name = autoScalerName,
										Resources = new ResourceRequirementsArgs
										{
											Limits = new InputMap<string>
											{
												{"cpu", "300m"},
												{"memory", "300Mi"}
											},
											Requests = new InputMap<string>
											{
												{"cpu", "300m"},
												{"memory", "300Mi"}
											}
										},
										Command = new InputList<string>
										{
											"./cluster-autoscaler",
											"--v=4",
											"--stderrthreshold=info",
											"--cloud-provider=aws",
											"--skip-nodes-with-local-storage=false",
											"--expander=least-waste",
											"--node-group-auto-discovery=asg:tag=k8s.io/cluster-autoscaler/enabled,k8s.io/cluster-autoscaler/ob-eks-cluster-demo",
											"--balance-similar-node-groups",
											"--skip-nodes-with-system-pods=false"
										},
										VolumeMounts = new InputList<VolumeMountArgs>
										{
											new VolumeMountArgs
											{
												Name = "ssl-certs",
												MountPath = "/etc/ssl/certs/ca-certificates.crt",
												ReadOnly = true
											}
										},
										
										ImagePullPolicy = "Always"
									}
									
								},
								Volumes = new InputList<VolumeArgs>
								{
									new VolumeArgs
									{
										Name = "ssl-certs",
										HostPath = new HostPathVolumeSourceArgs
										{
											Path = "/etc/ssl/certs/ca-bundle.crt"
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