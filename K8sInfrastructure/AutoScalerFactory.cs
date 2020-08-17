using K8sDemo.Models;
using Pulumi;
using Pulumi.Kubernetes.Types.Inputs.Autoscaling.V2Beta2;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;

namespace K8sDemo
{
	public class AutoScalerFactory
	{
		public AutoScalerFactory(AutoScalerFactoryArgs args)
		{
			var autoScalingService = new Pulumi.Kubernetes.Autoscaling.V2Beta2.HorizontalPodAutoscaler("hpa-example", new HorizontalPodAutoscalerArgs()
        {
            Metadata = args.Metadata,
            Spec = new HorizontalPodAutoscalerSpecArgs()
            {
                ScaleTargetRef = new CrossVersionObjectReferenceArgs
                {
                    Kind = args.TargetKind,
                    Name = args.TargetName,
                    ApiVersion = "apps/v1"
                },
                MinReplicas = args.MinReplicas,
                MaxReplicas = args.MaxReplicas,
                Metrics = new MetricSpecArgs
                {
                    Type = "Resource",
                    Resource = new ResourceMetricSourceArgs
                    {
                        Name = "cpu",
                        Target = new MetricTargetArgs
                        {
                            Type = "Utilization",
                            AverageUtilization = args.AverageUtilizationTarget
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
                                Value = args.ScaleDownTargetUtilization,
                                PeriodSeconds = args.PeriodSeconds
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
                                Value = args.ScaleUpUtilizationTarget,
                                PeriodSeconds = args.PeriodSeconds
                            }
                        }
                    }
                }
                
            }
        });
		}
	}
}