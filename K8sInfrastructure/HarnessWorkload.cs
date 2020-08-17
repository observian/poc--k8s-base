using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.Yaml;

namespace K8sDemo
{
	public class HarnessWorkload
	{
		public HarnessWorkload(Provider provider)
		{
			var harness = new ConfigFile("harness", new ConfigFileArgs
			{
				File = "harness-delegate.yaml"
			}, new ComponentResourceOptions
			{
				Provider = provider
			});
		}
	}
}