# Deployment Guide

## Production Deployment

This guide covers deploying Dog Teams to production environments.

## Deployment Options

### Option 1: Azure Container Instances (Recommended)

Simplest option for small to medium deployments.

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Docker Desktop

#### Steps

1. **Build Docker Image**
```bash
cd src/DogTeams.Api
docker build -t dogteams-api:latest .
docker build -t dogteams-web:latest ../DogTeams.Web/
```

2. **Push to Azure Container Registry**
```bash
az acr create --resource-group myResourceGroup --name dogteamsregistry --sku Basic

az acr build --registry dogteamsregistry --image dogteams-api:latest .
```

3. **Deploy to Container Instances**
```bash
az container create \
  --resource-group myResourceGroup \
  --name dogteams-api \
  --image dogteamsregistry.azurecr.io/dogteams-api:latest \
  --cpu 1 --memory 1 \
  --port 5000 \
  --registry-login-server dogteamsregistry.azurecr.io \
  --registry-username <username> \
  --registry-password <password> \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    JWT_SECRET_KEY=<your-secret-key> \
    COSMOSDB_CONNECTIONSTRING=<cosmos-db-connection> \
    REDIS_CONNECTIONSTRING=<redis-connection>
```

### Option 2: Azure App Service

Best for enterprise deployments with scaling.

#### Steps

1. **Create App Service Plan**
```bash
az appservice plan create \
  --name dogteams-plan \
  --resource-group myResourceGroup \
  --sku B1 \
  --is-linux
```

2. **Create Web App**
```bash
az webapp create \
  --resource-group myResourceGroup \
  --plan dogteams-plan \
  --name dogteams-api \
  --runtime "DOTNETCORE|8.0"
```

3. **Configure Environment Variables**
```bash
az webapp config appsettings set \
  --resource-group myResourceGroup \
  --name dogteams-api \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    JWT_SECRET_KEY="<your-secret-key>" \
    COSMOSDB_CONNECTIONSTRING="<connection-string>" \
    REDIS_CONNECTIONSTRING="<connection-string>"
```

4. **Deploy Code**
```bash
# Using zip deployment
cd src/DogTeams.Api
dotnet publish -c Release -o publish/
cd publish
zip -r ../dogteams-api.zip .

az webapp deployment source config-zip \
  --resource-group myResourceGroup \
  --name dogteams-api \
  --src ../dogteams-api.zip
```

### Option 3: Kubernetes (Advanced)

For large-scale, multi-region deployments.

#### Prerequisites
- Kubernetes cluster (AKS, EKS, GKE)
- kubectl configured
- Docker image in container registry

#### Kubernetes Manifests

**api-deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: dogteams-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: dogteams-api
  template:
    metadata:
      labels:
        app: dogteams-api
    spec:
      containers:
      - name: api
        image: myregistry.azurecr.io/dogteams-api:latest
        ports:
        - containerPort: 5000
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: Production
        - name: JWT_SECRET_KEY
          valueFrom:
            secretKeyRef:
              name: dogteams-secrets
              key: jwt-key
        - name: COSMOSDB_CONNECTIONSTRING
          valueFrom:
            secretKeyRef:
              name: dogteams-secrets
              key: cosmosdb-conn
        - name: REDIS_CONNECTIONSTRING
          valueFrom:
            secretKeyRef:
              name: dogteams-secrets
              key: redis-conn
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /api/health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /api/health
            port: 5000
          initialDelaySeconds: 10
          periodSeconds: 5

---
apiVersion: v1
kind: Service
metadata:
  name: dogteams-api
spec:
  type: LoadBalancer
  selector:
    app: dogteams-api
  ports:
  - port: 80
    targetPort: 5000
```

#### Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace dogteams

# Create secrets
kubectl create secret generic dogteams-secrets \
  --from-literal=jwt-key="<your-secret-key>" \
  --from-literal=cosmosdb-conn="<connection-string>" \
  --from-literal=redis-conn="<connection-string>" \
  -n dogteams

# Apply manifests
kubectl apply -f api-deployment.yaml -n dogteams

# Verify deployment
kubectl get pods -n dogteams
kubectl get svc -n dogteams
```

## Database & Cache Setup

### Cosmos DB (Production)

```bash
# Create Cosmos DB account
az cosmosdb create \
  --name dogteams-db \
  --resource-group myResourceGroup \
  --locations regionName=eastus failoverPriority=0

# Create database
az cosmosdb sql database create \
  --account-name dogteams-db \
  --resource-group myResourceGroup \
  --name dogteams

# Create containers with partition keys
az cosmosdb sql container create \
  --account-name dogteams-db \
  --database-name dogteams \
  --name users \
  --partition-key-path /userId \
  --throughput 400

az cosmosdb sql container create \
  --account-name dogteams-db \
  --database-name dogteams \
  --name teams \
  --partition-key-path /teamId \
  --throughput 400

# Get connection string
az cosmosdb list-connection-strings \
  --name dogteams-db \
  --resource-group myResourceGroup
```

### Redis Cache (Production)

```bash
# Create Redis instance
az redis create \
  --resource-group myResourceGroup \
  --name dogteams-cache \
  --location eastus \
  --sku Basic \
  --vm-size c0

# Enable SSL
az redis update \
  --name dogteams-cache \
  --resource-group myResourceGroup \
  --minimum-tls-version 1.2

# Get connection details
az redis show-connection-string \
  --name dogteams-cache \
  --resource-group myResourceGroup
```

## Security Configuration

### SSL/TLS Certificates

```bash
# Use Azure Key Vault for certificate management
az keyvault create \
  --name dogteams-vault \
  --resource-group myResourceGroup

# Import certificate
az keyvault certificate import \
  --vault-name dogteams-vault \
  --name dogteams-cert \
  --file dogteams.pfx \
  --password <password>
```

### Network Security

```bash
# Create Network Security Group
az network nsg create \
  --resource-group myResourceGroup \
  --name dogteams-nsg

# Allow HTTPS only
az network nsg rule create \
  --resource-group myResourceGroup \
  --nsg-name dogteams-nsg \
  --name AllowHTTPS \
  --priority 100 \
  --direction Inbound \
  --access Allow \
  --protocol Tcp \
  --source-address-prefixes '*' \
  --source-port-ranges '*' \
  --destination-address-prefixes '*' \
  --destination-port-ranges 443

# Block HTTP
az network nsg rule create \
  --resource-group myResourceGroup \
  --nsg-name dogteams-nsg \
  --name BlockHTTP \
  --priority 101 \
  --direction Inbound \
  --access Deny \
  --protocol Tcp \
  --source-address-prefixes '*' \
  --source-port-ranges '*' \
  --destination-address-prefixes '*' \
  --destination-port-ranges 80
```

## Environment Configuration

### Production Environment Variables

Create `.env.production` in the deployment:

```env
# Core
ASPNETCORE_ENVIRONMENT=Production

# Security
JWT_SECRET_KEY=<generate-32-char-minimum-random-key>
JWT_ISSUER=DogTeamsAPI
JWT_AUDIENCE=DogTeamsApp

# Database
COSMOSDB_CONNECTIONSTRING=<azure-cosmos-connection-string>
COSMOSDB_DATABASE=dogteams

# Cache
REDIS_CONNECTIONSTRING=<azure-redis-connection-string>

# CORS
CORS_ORIGINS=https://dogteams.example.com

# Logging
LOG_LEVEL=Information
LOG_FILE=/var/log/dogteams/api.log

# APM (Optional)
APPLICATIONINSIGHTS_CONNECTION_STRING=<app-insights-key>
```

## Monitoring & Logging

### Application Insights

```bash
# Create Application Insights resource
az monitor app-insights component create \
  --app dogteams-insights \
  --location eastus \
  --kind web \
  --resource-group myResourceGroup

# Get instrumentation key
az monitor app-insights component show \
  --app dogteams-insights \
  --resource-group myResourceGroup \
  --query instrumentationKey
```

### Log Aggregation

Configure Serilog in `appsettings.Production.json`:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/dogteams/api-.log",
          "rollingInterval": "Day",
          "shared": true
        }
      },
      {
        "Name": "ApplicationInsights",
        "Args": {
          "instrumentationKey": "<app-insights-key>"
        }
      }
    ]
  }
}
```

## CI/CD Pipeline

### GitHub Actions Workflow

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Build Docker image
      run: |
        docker build -t dogteams-api:${{ github.sha }} -f src/DogTeams.Api/Dockerfile .
        docker tag dogteams-api:${{ github.sha }} dogteams-api:latest
    
    - name: Login to Azure
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}
    
    - name: Push to Azure Container Registry
      run: |
        az acr build \
          --registry dogteamsregistry \
          --image dogteams-api:${{ github.sha }} \
          --file src/DogTeams.Api/Dockerfile .
    
    - name: Deploy to App Service
      uses: azure/webapps-deploy@v2
      with:
        app-name: dogteams-api
        images: dogteamsregistry.azurecr.io/dogteams-api:${{ github.sha }}
```

## Health Checks & Rollback

### Health Check Endpoint

```bash
# Check API health
curl https://dogteams.example.com/api/health

# Response
{
  "status": "healthy",
  "timestamp": "2026-03-07T14:00:00Z",
  "database": "connected",
  "cache": "connected"
}
```

### Rollback Procedure

```bash
# Using Azure App Service
az webapp deployment slot swap \
  --resource-group myResourceGroup \
  --name dogteams-api \
  --slot staging

# Using Kubernetes
kubectl rollout undo deployment/dogteams-api -n dogteams
kubectl rollout history deployment/dogteams-api -n dogteams
```

## Backup & Disaster Recovery

### Cosmos DB Backup

```bash
# Enable continuous backup (default)
az cosmosdb create \
  --name dogteams-db \
  --resource-group myResourceGroup \
  --backup-policy-type Continuous

# Restore from backup
az cosmosdb restore \
  --account-name dogteams-db-restored \
  --source-account-name dogteams-db \
  --restore-timestamp "2026-03-07T12:00:00Z" \
  --resource-group myResourceGroup
```

### Redis Backup

```bash
# Enable persistence
az redis update \
  --name dogteams-cache \
  --resource-group myResourceGroup \
  --enable-non-ssl-port true \
  --minimum-tls-version 1.2

# Export data
redis-cli --rdb /backups/dump.rdb
```

## Performance Tuning

### Cosmos DB Tuning

```bash
# Increase RU/s for peak loads
az cosmosdb sql database throughput update \
  --account-name dogteams-db \
  --database-name dogteams \
  --resource-group myResourceGroup \
  --throughput 2000

# Use auto-scaling
az cosmosdb sql database throughput migrate \
  --account-name dogteams-db \
  --database-name dogteams \
  --resource-group myResourceGroup \
  --throughput-type autoscale
```

### CDN Configuration

```bash
# Create CDN endpoint for static assets
az cdn endpoint create \
  --name dogteams-cdn \
  --profile-name dogteams-profile \
  --resource-group myResourceGroup \
  --origin dogteams.example.com \
  --origin-host-header dogteams.example.com \
  --cache-behaviors query-string-caching-behaviour IgnoreQueryString
```

## Troubleshooting Deployments

### Common Issues

**503 Service Unavailable**
- Check if all containers are running
- Verify database connectivity
- Check API logs

**High latency**
- Monitor Cosmos DB RU consumption
- Check Redis connection pool
- Review application logs for slow queries

**Authentication failures**
- Verify JWT_SECRET_KEY is set correctly
- Check token TTL settings
- Ensure clock synchronization

### Logs

```bash
# View App Service logs
az webapp log tail --name dogteams-api --resource-group myResourceGroup

# View Container logs
az container logs --resource-group myResourceGroup --name dogteams-api

# View Kubernetes logs
kubectl logs deployment/dogteams-api -n dogteams
```

## Post-Deployment

1. Run smoke tests
2. Verify all endpoints accessible
3. Check database connectivity
4. Test cache operations
5. Monitor application metrics
6. Setup alerts for failures
7. Document any configuration changes
