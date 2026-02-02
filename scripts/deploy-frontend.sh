#!/bin/bash

# Exit on error
set -e

echo "📦 Building React app..."
cd GM_Buddy.React
npm run build

echo "☁️  Uploading to S3..."
aws s3 sync dist/ s3://gm-buddy-frontend/ --delete

echo "🔄 Invalidating CloudFront cache..."
DIST_ID="E12QARTDZHFFZK"  # Replace with your distribution ID
aws cloudfront create-invalidation --distribution-id "$DIST_ID" --paths '/*' 

echo "✅ Deployment complete!"
echo "🌐 Your site: https://d2zsk9max2no60.cloudfront.net"