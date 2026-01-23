# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Build arguments for Cognito configuration
ARG VITE_COGNITO_DOMAIN=""
ARG VITE_COGNITO_CLIENT_ID=""
ARG VITE_COGNITO_REDIRECT_URI=""
ARG VITE_COGNITO_LOGOUT_URI=""
ARG VITE_USE_COGNITO="false"

# Set environment variables for build
ENV VITE_COGNITO_DOMAIN=$VITE_COGNITO_DOMAIN
ENV VITE_COGNITO_CLIENT_ID=$VITE_COGNITO_CLIENT_ID
ENV VITE_COGNITO_REDIRECT_URI=$VITE_COGNITO_REDIRECT_URI
ENV VITE_COGNITO_LOGOUT_URI=$VITE_COGNITO_LOGOUT_URI
ENV VITE_USE_COGNITO=$VITE_USE_COGNITO

# Copy package files
COPY package*.json ./

# Install dependencies (use npm install for flexibility with package-lock.json)
RUN npm install --legacy-peer-deps

# Copy source files
COPY . .

# Build the application
RUN npm run build

# Production stage with nginx
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
