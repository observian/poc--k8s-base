using Pulumi.Kubernetes.Yaml;

namespace K8sDemo
{
	public class HarnessWorkload
	{
		public HarnessWorkload()
		{
			var harness = new ConfigFile("harness", new ConfigFileArgs
			{
				File = "harness-delegate.yaml"
			});
		}
	}
}