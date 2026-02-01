#!/bin/bash

# Exit on error
set -e

echo "📦 Building React app..."
cd GM_Buddy.React
npm run build

echo "☁️  Uploading to S3..."
aws s3 sync dist/ s3://gm-buddy-frontend/ --delete

echo "🔄 Invalidating CloudFront cache..."
DIST_ID="${CLOUDFRONT_DIST_ID:?CLOUDFRONT_DIST_ID environment variable must be set}"
aws cloudfront create-invalidation --distribution-id "$DIST_ID" --paths '/*'

echo "✅ Deployment complete!"
if [ -n "${CLOUDFRONT_URL:-}" ]; then
  echo "🌐 Your site: $CLOUDFRONT_URL"
else
  echo "🌐 Deployment complete. Set CLOUDFRONT_URL to print the site URL."
fi