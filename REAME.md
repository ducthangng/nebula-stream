# Nebula Stream
Nebula is a provider-agnostic "Outsourcing OS" built to automate the full lifecycle of serverless and containerized applications.

### Core Tech: Built with Go, TypeScript, and .NET 8, utilizing Pulumi for Infrastructure-as-Code (IaC).
- Key Features: Features a custom CLI for rapid project scaffolding, a Kubernetes Operator for managing ephemeral PR environments, and a robust multi-tenant architecture that ensures strict data isolation between clients.
- Innovation: Integrates an AI-driven management mesh to monitor resource consumption and automate cost-saving measures across hybrid-cloud deployments.


## Phase 1: The Nebula CLI & Core Scaffolding
The CLI is the primary entry point for your developers to interact with the platform without needing to touch AWS or GCP consoles.

### Step 1.1: CLI Command Design & Auth
Build the base CLI using a library like Cobra (Go) or oclif (TypeScript). Implement OIDC authentication to link the CLI with your cloud providers.
- Input: User credentials, Cloud Provider config (IAM roles/Service Accounts).
- Output: An authenticated session and a .nebula config file in the user's home directory.

### Step 1.2: Project Initialization Engine
What: Implement nebula init to scaffold projects based on your "Outsourcing OS" templates (e.g., .NET 8 backends or Go workers).
- Input: Project name, target cloud (AWS/GCP), and architectural template choice.
- Output: A localized git repo with predefined folder structures and nebula.yaml metadata.

## Phase 2: Multi-Cloud Provisioning (AWS & GCP)
Using the Pulumi SDK within your Provisioning Engine (.NET 8) to ensure provider-agnosticism.

### Step 2.1: Cloud Provider Abstraction Layer
What: Define interfaces for common resources (Databases, Buckets, Compute) so the CLI can call nebula up regardless of the backend.
- Input: Abstract resource definitions from nebula.yaml.
- Output: A provider-specific execution plan (CloudFormation for AWS, Deployment Manager for GCP).

### Step 2.2: AWS Implementation (FinOps Focus)
What: Provision EKS/Lambda and RDS using Pulumi AWS. Ensure all resources are tagged for the AI-Driven FinOps pillar.
- Input: Pulumi AWS SDK + Tenant ID.
- Output: Live infrastructure on AWS with cost-tracking tags.

### Step 2.3: GCP Implementation (Data/AI Focus)
What: Provision GKE or Cloud Run and BigQuery using Pulumi GCP. Focus on setting up IAM Workload Identity for security.
- Input: Pulumi GCP SDK + Service Account JSON.
- Output: Live infrastructure on GCP with restricted IAM scopes.

## Phase 3: Tenant Isolation & The Nebula Operator
This ensures that Client A's data and infrastructure never touch Client B's.

### Step 3.1: Kubernetes Operator Development (Go)
What: Build the Nebula Operator using the K8s Controller Runtime to manage the lifecycle of ephemeral environments and tenant namespaces.
- Input: Custom Resource Definitions (CRDs) like NebulaTenant or NebulaApp.
- Output: A running controller in your management cluster that automates namespace/RBAC creation.

### Step 3.2: Network & Identity Isolation
What: Use VPC Peering or Private Service Connect to isolate tenant traffic.
- Input: Network CIDR blocks and Tenant Security Policies.
- Output: Isolated VPCs or Subnets per client project.

## Phase 4: Writing Testing & Quality Assurance
You mentioned "Ephemeral Environments" and "Audit Logging" as key requirements.

### Step 4.1: Unit & Integration Testing for IaC
What: Write Pulumi policy tests (Sentinel/Crossguard) to ensure no developer creates public S3 buckets or unencrypted databases.
- Input: Infrastructure code (TypeScript/C#).
- Output: Pass/Fail reports during the nebula preview stage.

### Step 4.2: Preview Environments (Ephemeral)
What: Triggered by a Pull Request, the CLI/CI pipeline spins up a mini-version of the stack.
- Input: PR Webhook + Pulumi Stack definition.
- Output: A temporary URL (e.g., pr-123.nebula.dev) for testing.

### Step 4.3: End-to-End (E2E) Smoke Tests
What: Automated scripts that verify the CLI can successfully deploy a "Hello World" app to both AWS and GCP.
- Input: Playwright or Go test scripts.
- Output: Deployment success metrics and logs.