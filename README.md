# Base Kubernetes Workload

This repo sets up a basic Kuberenetes workload with a sample dotnet core application which has been set up for Horizontal Pod Autoscaling (HPA).

## Usage

[Install Pulumi](https://www.pulumi.com/docs/get-started/install/)


Make sure you have the `kubeconfig` from your cluster in your current `kubectl` context

Navigate to the K8sInfrastructure folder


Run `pulumi up`