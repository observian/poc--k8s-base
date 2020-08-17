using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Pulumi.Kubernetes.Types.Outputs.Meta.V1;

namespace K8sDemo.Models
{
	public class AutoScalerFactoryArgs
	{
		public ObjectMetaArgs Metadata { get; set; }
		public string Namespace { get; set; }
		public string TargetKind { get; set; }
		public string TargetName { get; set; }
		public int AverageUtilizationTarget { get; set; }
		public int ScaleDownTargetUtilization { get; set; }
		public int ScaleUpUtilizationTarget { get; set; }
		public int PeriodSeconds { get; set; }
		public int MinReplicas { get; set; }
		public int MaxReplicas { get; set; }
	}
}