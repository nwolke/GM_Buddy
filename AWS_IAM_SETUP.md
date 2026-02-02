# AWS IAM Setup for Deployment User

This guide explains how to create an AWS IAM user with the minimum required permissions to deploy GM_Buddy to AWS.

**📄 Ready-to-use policy file**: [`gm-buddy-deploy-policy.json`](./gm-buddy-deploy-policy.json)

---

## Overview

The deployment workflows need permissions for:
- **ECR** (Elastic Container Registry) - Push Docker images
- **Elastic Beanstalk** - Deploy and manage backend application
- **S3** - Upload and sync frontend files
- **CloudFront** - Invalidate CDN cache

---

## Quick Start

### Option 1: Use AWS Managed Policies (Easier, Less Secure)

**Not Recommended for Production** - These policies grant more permissions than needed.

Attach these AWS managed policies to your deployment user:
- `AmazonEC2ContainerRegistryPowerUser`
- `AdministratorAccess-AWSElasticBeanstalk`
- `AmazonS3FullAccess`
- `CloudFrontFullAccess`

### Option 2: Custom Policy (Recommended, Least Privilege)

Create a custom policy with only the required permissions. See below for details.

---

## Step-by-Step: Create Deployment User

### 1. Create IAM User

```bash
# Using AWS CLI
aws iam create-user --user-name gm-buddy-deploy

# Or use AWS Console:
# IAM → Users → Add users → User name: gm-buddy-deploy
```

### 2. Create Access Keys

```bash
# Using AWS CLI
aws iam create-access-key --user-name gm-buddy-deploy

# Or use AWS Console:
# IAM → Users → gm-buddy-deploy → Security credentials → Create access key
# → Use case: Application running outside AWS → Create access key
```

**Save the credentials:**
- Access Key ID → Use as `AWS_ACCESS_KEY_ID` GitHub secret
- Secret Access Key → Use as `AWS_SECRET_ACCESS_KEY` GitHub secret

### 3. Attach Policy

The repository includes a ready-to-use IAM policy file: **`gm-buddy-deploy-policy.json`**

**Before using it:**
1. Open `gm-buddy-deploy-policy.json` in a text editor
2. Replace `YOUR_FRONTEND_BUCKET_NAME` with your actual S3 bucket name (appears in 2 places)
3. Optionally, replace `YOUR_ACCOUNT_ID` in the commands below with your AWS account ID

**Create and attach the policy:**

```bash
# Create the policy from the file
aws iam create-policy \
  --policy-name GMBuddyDeploymentPolicy \
  --policy-document file://gm-buddy-deploy-policy.json

# Attach to user (replace YOUR_ACCOUNT_ID with your 12-digit AWS account ID)
aws iam attach-user-policy \
  --user-name gm-buddy-deploy \
  --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/GMBuddyDeploymentPolicy
```

**Or use AWS Console:**
1. IAM → Policies → Create policy
2. JSON tab → Copy content from `gm-buddy-deploy-policy.json` (after editing)
3. Review + Create → Name: `GMBuddyDeploymentPolicy`
4. IAM → Users → gm-buddy-deploy → Add permissions → Attach policies → Select GMBuddyDeploymentPolicy

---

## Required IAM Policy (Least Privilege)

The repository includes **`gm-buddy-deploy-policy.json`** with the complete IAM policy.

**Policy content** (also available in `gm-buddy-deploy-policy.json`):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "ECRPermissions",
      "Effect": "Allow",
      "Action": [
        "ecr:GetAuthorizationToken",
        "ecr:BatchCheckLayerAvailability",
        "ecr:GetDownloadUrlForLayer",
        "ecr:BatchGetImage",
        "ecr:PutImage",
        "ecr:InitiateLayerUpload",
        "ecr:UploadLayerPart",
        "ecr:CompleteLayerUpload"
      ],
      "Resource": "*"
    },
    {
      "Sid": "ElasticBeanstalkDeploy",
      "Effect": "Allow",
      "Action": [
        "elasticbeanstalk:CreateApplicationVersion",
        "elasticbeanstalk:DescribeApplicationVersions",
        "elasticbeanstalk:DescribeEnvironments",
        "elasticbeanstalk:DescribeEvents",
        "elasticbeanstalk:UpdateEnvironment",
        "elasticbeanstalk:DescribeEnvironmentHealth",
        "elasticbeanstalk:DescribeInstancesHealth"
      ],
      "Resource": [
        "arn:aws:elasticbeanstalk:*:*:application/*",
        "arn:aws:elasticbeanstalk:*:*:applicationversion/*/*",
        "arn:aws:elasticbeanstalk:*:*:environment/*/*"
      ]
    },
    {
      "Sid": "S3UploadToDeploymentBucket",
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject"
      ],
      "Resource": "arn:aws:s3:::elasticbeanstalk-*/*"
    },
    {
      "Sid": "S3FrontendBucket",
      "Effect": "Allow",
      "Action": [
        "s3:ListBucket",
        "s3:GetBucketLocation"
      ],
      "Resource": "arn:aws:s3:::YOUR_FRONTEND_BUCKET_NAME"
    },
    {
      "Sid": "S3FrontendObjects",
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:PutObjectAcl",
        "s3:GetObject",
        "s3:DeleteObject"
      ],
      "Resource": "arn:aws:s3:::YOUR_FRONTEND_BUCKET_NAME/*"
    },
    {
      "Sid": "CloudFrontInvalidation",
      "Effect": "Allow",
      "Action": [
        "cloudfront:CreateInvalidation",
        "cloudfront:GetInvalidation"
      ],
      "Resource": "arn:aws:cloudfront::*:distribution/*"
    },
    {
      "Sid": "AutoScalingRead",
      "Effect": "Allow",
      "Action": [
        "autoscaling:DescribeAutoScalingGroups",
        "autoscaling:DescribeAutoScalingInstances",
        "autoscaling:DescribeScalingActivities",
        "autoscaling:DescribeLaunchConfigurations"
      ],
      "Resource": "*"
    },
    {
      "Sid": "EC2Read",
      "Effect": "Allow",
      "Action": [
        "ec2:DescribeInstances",
        "ec2:DescribeImages",
        "ec2:DescribeSecurityGroups",
        "ec2:DescribeSubnets"
      ],
      "Resource": "*"
    },
    {
      "Sid": "LoadBalancerRead",
      "Effect": "Allow",
      "Action": [
        "elasticloadbalancing:DescribeLoadBalancers",
        "elasticloadbalancing:DescribeTargetGroups",
        "elasticloadbalancing:DescribeTargetHealth"
      ],
      "Resource": "*"
    }
  ]
}
```

**Important**: Replace the following placeholders:
- `YOUR_ACCOUNT_ID` - Your AWS account ID (12 digits)
- `YOUR_FRONTEND_BUCKET_NAME` - Your S3 bucket name (e.g., `gm-buddy-frontend`)

---

## Customizing the Policy

### Restrict to Specific Resources

For better security, restrict permissions to specific resources:

```json
{
  "Sid": "ECRSpecificRepository",
  "Effect": "Allow",
  "Action": [
    "ecr:BatchCheckLayerAvailability",
    "ecr:GetDownloadUrlForLayer",
    "ecr:BatchGetImage",
    "ecr:PutImage",
    "ecr:InitiateLayerUpload",
    "ecr:UploadLayerPart",
    "ecr:CompleteLayerUpload"
  ],
  "Resource": "arn:aws:ecr:us-west-2:YOUR_ACCOUNT_ID:repository/gm-buddy-server"
}
```

### Restrict Elastic Beanstalk to Specific Application

```json
{
  "Sid": "ElasticBeanstalkSpecificApp",
  "Effect": "Allow",
  "Action": [
    "elasticbeanstalk:*"
  ],
  "Resource": [
    "arn:aws:elasticbeanstalk:us-west-2:YOUR_ACCOUNT_ID:application/gm-buddy-server",
    "arn:aws:elasticbeanstalk:us-west-2:YOUR_ACCOUNT_ID:applicationversion/gm-buddy-server/*",
    "arn:aws:elasticbeanstalk:us-west-2:YOUR_ACCOUNT_ID:environment/gm-buddy-server/*"
  ]
}
```

### Restrict CloudFront to Specific Distribution

```json
{
  "Sid": "CloudFrontSpecificDistribution",
  "Effect": "Allow",
  "Action": [
    "cloudfront:CreateInvalidation",
    "cloudfront:GetInvalidation"
  ],
  "Resource": "arn:aws:cloudfront::YOUR_ACCOUNT_ID:distribution/YOUR_DISTRIBUTION_ID"
}
```

---

## Policy Breakdown

### ECR Permissions

Required for pushing Docker images to ECR:

- `ecr:GetAuthorizationToken` - Login to ECR
- `ecr:BatchCheckLayerAvailability` - Check if layers exist
- `ecr:PutImage` - Upload Docker image
- `ecr:InitiateLayerUpload`, `ecr:UploadLayerPart`, `ecr:CompleteLayerUpload` - Upload image layers

### Elastic Beanstalk Permissions

Required for deploying backend to Elastic Beanstalk:

- `elasticbeanstalk:CreateApplicationVersion` - Create new version
- `elasticbeanstalk:UpdateEnvironment` - Deploy new version
- `elasticbeanstalk:Describe*` - Check deployment status

### S3 Permissions

Required for uploading frontend files:

- `s3:ListBucket` - List bucket contents
- `s3:PutObject` - Upload files
- `s3:DeleteObject` - Remove old files (for sync --delete)
- `s3:PutObjectAcl` - Set file permissions

Also required for Elastic Beanstalk deployment packages:
- Access to `elasticbeanstalk-*` buckets

### CloudFront Permissions

Required for cache invalidation:

- `cloudfront:CreateInvalidation` - Invalidate cache after deployment
- `cloudfront:GetInvalidation` - Check invalidation status

### Read-Only Permissions

Required for Elastic Beanstalk to check deployment status:

- AutoScaling, EC2, and Load Balancer describe permissions

---

## Verification

Test that your IAM user has the correct permissions:

```bash
# Configure AWS CLI with the new credentials
aws configure --profile gm-buddy-deploy

# Test ECR access
aws ecr describe-repositories --profile gm-buddy-deploy

# Test Elastic Beanstalk access
aws elasticbeanstalk describe-environments --profile gm-buddy-deploy

# Test S3 access
aws s3 ls s3://YOUR_FRONTEND_BUCKET_NAME --profile gm-buddy-deploy

# Test CloudFront access
aws cloudfront list-distributions --profile gm-buddy-deploy
```

---

## Security Best Practices

### 1. Use Least Privilege

Only grant the minimum permissions required. Start with the policy above and adjust based on your specific needs.

### 2. Rotate Access Keys Regularly

```bash
# Create new access key
aws iam create-access-key --user-name gm-buddy-deploy

# Update GitHub secrets with new credentials

# Delete old access key
aws iam delete-access-key \
  --user-name gm-buddy-deploy \
  --access-key-id OLD_ACCESS_KEY_ID
```

### 3. Enable MFA (Optional)

For additional security, require MFA for the IAM user:

```bash
aws iam enable-mfa-device \
  --user-name gm-buddy-deploy \
  --serial-number arn:aws:iam::YOUR_ACCOUNT_ID:mfa/gm-buddy-deploy \
  --authentication-code1 CODE1 \
  --authentication-code2 CODE2
```

### 4. Monitor Usage

Set up CloudWatch alarms for unusual API activity:

```bash
aws cloudwatch put-metric-alarm \
  --alarm-name gm-buddy-deploy-unusual-activity \
  --alarm-description "Alert on unusual API calls" \
  --metric-name CallCount \
  --namespace AWS/Usage \
  --statistic Sum \
  --period 3600 \
  --threshold 1000 \
  --comparison-operator GreaterThanThreshold
```

### 5. Use AWS Organizations Service Control Policies (SCPs)

If using AWS Organizations, add an SCP to restrict regions:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Deny",
      "Action": "*",
      "Resource": "*",
      "Condition": {
        "StringNotEquals": {
          "aws:RequestedRegion": ["us-west-2"]
        }
      }
    }
  ]
}
```

---

## Troubleshooting

### "Access Denied" Errors

If you get access denied errors during deployment:

1. **Check the policy is attached:**
   ```bash
   aws iam list-attached-user-policies --user-name gm-buddy-deploy
   ```

2. **Verify the policy document:**
   ```bash
   aws iam get-policy-version \
     --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/GMBuddyDeploymentPolicy \
     --version-id v1
   ```

3. **Check CloudTrail logs** to see which specific permission is missing:
   ```bash
   aws cloudtrail lookup-events \
     --lookup-attributes AttributeKey=Username,AttributeValue=gm-buddy-deploy
   ```

### ECR Login Issues

```
Error: Cannot perform an interactive login from a non TTY device
```

**Solution**: Ensure `ecr:GetAuthorizationToken` permission is granted and resource is set to `"*"`

### Elastic Beanstalk Deployment Failures

```
Error: User is not authorized to perform: elasticbeanstalk:UpdateEnvironment
```

**Solution**: Add the specific environment ARN to the policy resource list

### S3 Access Issues

```
Error: Access Denied when accessing S3 bucket
```

**Solution**: Verify both bucket-level and object-level permissions are granted

---

## Alternative: Using IAM Roles (Advanced)

For better security, use OIDC to federate GitHub Actions with AWS IAM roles:

### Benefits
- No long-lived credentials
- Automatic credential rotation
- Scoped to specific GitHub repository/branch

### Setup

1. **Create OIDC Provider in AWS:**
   ```bash
   aws iam create-open-id-connect-provider \
     --url https://token.actions.githubusercontent.com \
     --client-id-list sts.amazonaws.com \
     --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1
   ```

2. **Create IAM Role with Trust Policy:**
   ```json
   {
     "Version": "2012-10-17",
     "Statement": [
       {
         "Effect": "Allow",
         "Principal": {
           "Federated": "arn:aws:iam::YOUR_ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com"
         },
         "Action": "sts:AssumeRoleWithWebIdentity",
         "Condition": {
           "StringEquals": {
             "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
             "token.actions.githubusercontent.com:sub": "repo:nwolke/GM_Buddy:ref:refs/heads/main"
           }
         }
       }
     ]
   }
   ```

3. **Attach the deployment policy to the role**

4. **Update GitHub Actions workflows** to use the role instead of access keys

**Note**: This is more complex but significantly more secure. See [GitHub's documentation](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/configuring-openid-connect-in-amazon-web-services) for details.

---

## Summary

### Minimum Required Permissions

```
✓ ECR - Push Docker images
✓ Elastic Beanstalk - Deploy applications
✓ S3 - Upload frontend files
✓ CloudFront - Invalidate cache
✓ AutoScaling, EC2, ELB - Read-only for status checks
```

### Quick Commands

```bash
# Create user
aws iam create-user --user-name gm-buddy-deploy

# Create access key
aws iam create-access-key --user-name gm-buddy-deploy

# Create policy (using JSON file)
aws iam create-policy \
  --policy-name GMBuddyDeploymentPolicy \
  --policy-document file://gm-buddy-deploy-policy.json

# Attach policy to user
aws iam attach-user-policy \
  --user-name gm-buddy-deploy \
  --policy-arn arn:aws:iam::YOUR_ACCOUNT_ID:policy/GMBuddyDeploymentPolicy
```

---

## Next Steps

1. ✅ Create IAM user with this policy
2. ✅ Save access key credentials
3. ✅ Add credentials to GitHub secrets (see [GITHUB_SECRETS.md](./GITHUB_SECRETS.md))
4. ✅ Test deployment (see [MANUAL_DEPLOYMENT.md](./MANUAL_DEPLOYMENT.md))

---

## References

- [AWS IAM Best Practices](https://docs.aws.amazon.com/IAM/latest/UserGuide/best-practices.html)
- [ECR IAM Policies](https://docs.aws.amazon.com/AmazonECR/latest/userguide/security-iam.html)
- [Elastic Beanstalk IAM Policies](https://docs.aws.amazon.com/elasticbeanstalk/latest/dg/AWSHowTo.iam.html)
- [S3 IAM Policies](https://docs.aws.amazon.com/AmazonS3/latest/userguide/access-policy-language-overview.html)
- [CloudFront IAM Policies](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/cf-api-permissions-ref.html)
