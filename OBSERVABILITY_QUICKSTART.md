# Quick Start: Observability Setup

This guide provides step-by-step instructions to enable observability features in GM_Buddy.

## Table of Contents
- [Local Development Setup](#local-development-setup)
- [Production AWS CloudWatch Setup](#production-aws-cloudwatch-setup)
- [Viewing Logs and Metrics](#viewing-logs-and-metrics)
- [Setting Up Alerts](#setting-up-alerts)

---

## Local Development Setup

### 1. Enable Logging

Logging is **enabled by default** in development. Logs are written to:
- **Console**: Real-time output in your terminal
- **Files**: `./logs/gm-buddy-YYYYMMDD.log` (rolling daily)

No configuration needed! Just run the application:

```bash
cd GM_Buddy.Server
dotnet run
```

### 2. View Real-time Metrics

Use `dotnet-counters` to monitor metrics while the app is running:

```bash
# Install dotnet-counters (one time)
dotnet tool install --global dotnet-counters

# Find the process ID
dotnet-counters ps

# Monitor metrics
dotnet-counters monitor -p <process-id> --counters GMBuddy.Application
```

You'll see real-time metrics like:
- Request count
- Request duration
- Memory usage
- CPU usage
- GC collections

### 3. Test Health Checks

While the app is running, test the health endpoints:

```bash
# Liveness check
curl http://localhost:8080/health/live

# Readiness check (includes database)
curl http://localhost:8080/health/ready

# Full health status
curl http://localhost:8080/health | jq '.'
```

---

## Production AWS CloudWatch Setup

### Prerequisites
- Application deployed to AWS Elastic Beanstalk
- AWS CLI configured
- Admin access to AWS account

### Step 1: Add IAM Permissions

Your Elastic Beanstalk instance role needs CloudWatch permissions.

**Option A: Using AWS CLI**

```bash
# Create the policy
aws iam create-policy \
  --policy-name GMBuddyCloudWatchPolicy \
  --policy-document file://cloudwatch-policy.json

# Attach to your EB instance profile role
aws iam attach-role-policy \
  --role-name aws-elasticbeanstalk-ec2-role \
  --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/GMBuddyCloudWatchPolicy
```

**Option B: Using AWS Console**

1. Go to IAM → Policies → Create Policy
2. Copy the content from `cloudwatch-policy.json`
3. Create policy named "GMBuddyCloudWatchPolicy"
4. Go to IAM → Roles → `aws-elasticbeanstalk-ec2-role`
5. Attach the new policy

### Step 2: Enable CloudWatch in Application

Update your `appsettings.Production.json` (via environment variables or Elastic Beanstalk configuration):

```json
{
  "AWS": {
    "CloudWatch": {
      "Enabled": true,
      "Region": "us-east-1",
      "LogGroup": "/aws/elasticbeanstalk/gm-buddy/application",
      "MetricsNamespace": "GMBuddy/Application"
    }
  }
}
```

**Using Elastic Beanstalk Environment Variables:**

```bash
eb setenv \
  AWS__CloudWatch__Enabled=true \
  AWS__CloudWatch__Region=us-east-1 \
  AWS__CloudWatch__LogGroup=/aws/elasticbeanstalk/gm-buddy/application \
  AWS__CloudWatch__MetricsNamespace=GMBuddy/Application
```

### Step 3: Deploy and Verify

1. Deploy your application
2. Wait 2-3 minutes for metrics to start flowing
3. Verify in AWS Console:
   - CloudWatch → Log Groups → `/aws/elasticbeanstalk/gm-buddy/application`
   - CloudWatch → Metrics → Custom Namespaces → `GMBuddy/Application`

---

## Viewing Logs and Metrics

### View Logs in CloudWatch

**AWS Console:**
1. Go to CloudWatch → Log Groups
2. Select `/aws/elasticbeanstalk/gm-buddy/application`
3. Click on the latest log stream
4. Use the filter box to search logs

**AWS CLI:**
```bash
# List log streams
aws logs describe-log-streams \
  --log-group-name /aws/elasticbeanstalk/gm-buddy/application \
  --order-by LastEventTime \
  --descending

# Tail logs
aws logs tail /aws/elasticbeanstalk/gm-buddy/application --follow
```

### View Metrics in CloudWatch

**AWS Console:**
1. Go to CloudWatch → Metrics → All Metrics
2. Select `GMBuddy/Application`
3. Choose metrics to graph:
   - `RequestDuration` - API response times
   - `MemoryUsage` - Application memory
   - `CPUUsage` - CPU utilization
   - `Errors` - Error count

**AWS CLI:**
```bash
# Get metric statistics
aws cloudwatch get-metric-statistics \
  --namespace GMBuddy/Application \
  --metric-name RequestDuration \
  --start-time 2026-02-07T00:00:00Z \
  --end-time 2026-02-07T23:59:59Z \
  --period 300 \
  --statistics Average,Maximum
```

### Create a CloudWatch Dashboard

**Using AWS Console:**
1. CloudWatch → Dashboards → Create Dashboard
2. Name it "GMBuddy-Production"
3. Add widgets:
   - **Line Graph**: Request Duration (Average, p95, p99)
   - **Number**: Current Error Count (Sum, last 5 minutes)
   - **Line Graph**: Memory Usage (Average)
   - **Line Graph**: CPU Usage (Average)
   - **Stacked Area**: Request Count by Method

---

## Setting Up Alerts

### Create Error Rate Alarm

**High Error Rate:**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-high-error-rate \
  --alarm-description "Alert when error rate exceeds threshold" \
  --metric-name Errors \
  --namespace GMBuddy/Application \
  --statistic Sum \
  --period 300 \
  --evaluation-periods 1 \
  --threshold 10 \
  --comparison-operator GreaterThanThreshold \
  --treat-missing-data notBreaching
```

### Create Memory Alarm

**High Memory Usage:**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-high-memory \
  --alarm-description "Alert when memory exceeds 500MB" \
  --metric-name MemoryUsage \
  --namespace GMBuddy/Application \
  --statistic Average \
  --period 300 \
  --evaluation-periods 2 \
  --threshold 500 \
  --comparison-operator GreaterThanThreshold \
  --treat-missing-data notBreaching
```

### Create Response Time Alarm

**Slow Response Times:**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-slow-response \
  --alarm-description "Alert when response time p95 > 1000ms" \
  --metric-name RequestDuration \
  --namespace GMBuddy/Application \
  --statistic Average \
  --period 300 \
  --evaluation-periods 2 \
  --threshold 1000 \
  --comparison-operator GreaterThanThreshold \
  --treat-missing-data notBreaching
```

### Set Up SNS Notifications

**Create SNS Topic:**
```bash
# Create topic
aws sns create-topic --name gm-buddy-alerts

# Subscribe email
aws sns subscribe \
  --topic-arn arn:aws:sns:us-east-1:YOUR_ACCOUNT_ID:gm-buddy-alerts \
  --protocol email \
  --notification-endpoint your-email@example.com
```

**Link Alarms to SNS:**
```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-high-error-rate \
  --alarm-actions arn:aws:sns:us-east-1:YOUR_ACCOUNT_ID:gm-buddy-alerts \
  # ... rest of alarm configuration
```

---

## Troubleshooting

### Logs not appearing in CloudWatch

1. **Check IAM permissions**: Ensure the instance profile has CloudWatch Logs permissions
2. **Verify configuration**: Check environment variables are set correctly
3. **Check application logs locally**: Look for CloudWatch errors in console output
4. **Check log group exists**: CloudWatch will create it automatically, but verify

```bash
aws logs describe-log-groups --log-group-name-prefix /aws/elasticbeanstalk/gm-buddy
```

### Metrics not appearing in CloudWatch

1. **Wait 2-3 minutes**: Initial metrics take time to appear
2. **Generate traffic**: Make some API requests to generate metrics
3. **Check IAM permissions**: Ensure CloudWatch PutMetricData permission exists
4. **Verify namespace**: Metrics appear under `GMBuddy/Application`

### Health checks failing after deployment

1. **Wait for startup**: Application may take 30-60 seconds to start
2. **Check database connectivity**: Readiness check requires database
3. **Review logs**: Check CloudWatch or local logs for errors

```bash
# Check health endpoint directly
curl https://your-app-url.elasticbeanstalk.com/health/live
```

---

## Next Steps

- Read the complete [OBSERVABILITY.md](./OBSERVABILITY.md) guide
- Set up a CloudWatch Dashboard
- Configure alerts for critical metrics
- Review logs regularly to understand application behavior
- Consider integrating with third-party tools (Datadog, New Relic, etc.)

## Cost Estimation

**Typical monthly costs for a small-medium application:**
- CloudWatch Logs: ~$5-20 (depends on log volume)
- CloudWatch Metrics: ~$3-10 (custom metrics)
- CloudWatch Alarms: ~$0.10 per alarm
- CloudWatch Dashboards: $3 per dashboard

**Total**: ~$10-40/month for comprehensive observability

**Cost optimization tips:**
- Set log retention to 7-30 days
- Filter logs before sending to CloudWatch
- Use metric filters to reduce custom metrics
- Use AWS Free Tier where available
