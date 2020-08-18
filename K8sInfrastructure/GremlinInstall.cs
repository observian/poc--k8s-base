using System;
using Pulumi;
using Pulumi.Kubernetes;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Helm;
using Pulumi.Kubernetes.Helm.V3;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using Config = Pulumi.Config;

namespace K8sDemo
{
	public class GremlinInstall
	{
		public GremlinInstall(Provider provider,  string clusterId, Config config)
		{
			var gremlinNamespace = new Namespace("gremlinNamespace", new NamespaceArgs
			{
				Metadata = new ObjectMetaArgs
				{
					Name = "gremlin"
				}
			}, new CustomResourceOptions
			{
				Provider = provider
			});

			//pulumi is not ready for helm chart installs as long as the secrets and config values are aysnc the way they are.
			 var gremllinTeamId = config.RequireSecret("GREMLIN_TEAM_ID").Apply(x => x.ToString());
			 Console.WriteLine($"team id is {gremllinTeamId}");
			
			 Console.WriteLine(config.RequireSecret("GREMLIN_TEAM_SECRET"));
			 var gremlinInstall = new Chart("gremlinChart", new ChartArgs
			 {
			 	Repo = "gremlin",
			 	Chart	= "gremlin",
			 	Namespace = "gremlin",
			 	Values = new InputMap<object>
			 	{
			 		{"gremlin.secret.managed", "true"},
			 		{"gremlin.secret.type","secret"},
			 		{"gremlin.secret.teamID", config.RequireSecret("GREMLIN_TEAM_ID")},
			 		{"gremlin.secret.clusterID",$"{clusterId}"},
			 		{"gremlin.secret.teamSecret", config.RequireSecret("GREMLIN_TEAM_SECRET")}
			 	},
			 	
			 }, new ComponentResourceOptions
			 {
			 	Provider = provider
			 });
		}
	}
}