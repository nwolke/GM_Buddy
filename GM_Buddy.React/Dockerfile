# ===========================================
# GM_Buddy.React - Local Development Dockerfile
# ===========================================
# Multi-stage build for React frontend with Nginx

# Build stage
FROM node:20-alpine AS build
WORKDIR /app

# Build arguments for environment variables
ARG VITE_API_URL=http://localhost:8080
ARG VITE_USE_COGNITO=false
ARG VITE_COGNITO_DOMAIN=""
ARG VITE_COGNITO_CLIENT_ID=""
ARG VITE_COGNITO_REDIRECT_URI=http://localhost:3000/callback
ARG VITE_COGNITO_LOGOUT_URI=http://localhost:3000

# Set environment variables for build
ENV VITE_API_URL=$VITE_API_URL
ENV VITE_USE_COGNITO=$VITE_USE_COGNITO
ENV VITE_COGNITO_DOMAIN=$VITE_COGNITO_DOMAIN
ENV VITE_COGNITO_CLIENT_ID=$VITE_COGNITO_CLIENT_ID
ENV VITE_COGNITO_REDIRECT_URI=$VITE_COGNITO_REDIRECT_URI
ENV VITE_COGNITO_LOGOUT_URI=$VITE_COGNITO_LOGOUT_URI

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --legacy-peer-deps

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage with Nginx
FROM nginx:alpine AS production

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port
EXPOSE 80

# Start Nginx
CMD ["nginx", "-g", "daemon off;"]
