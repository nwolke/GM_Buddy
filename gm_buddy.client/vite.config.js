import { fileURLToPath, URL } from 'node:url';

import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-react';
import fs from 'fs';
import path from 'path';
import child_process from 'child_process';
import { env } from 'process';

const baseFolder =
    env.APPDATA !== undefined && env.APPDATA !== ''
        ? `${env.APPDATA}/ASP.NET/https`
        : `${env.HOME}/.aspnet/https`;

const certificateName = "gm_buddy.client";
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// https://vitejs.dev/config/
export default defineConfig(async () => {
    // default config values
    let useHttps = false;

    try {
        if (!fs.existsSync(baseFolder)) {
            try {
                fs.mkdirSync(baseFolder, { recursive: true });
            } catch (e) {
                // ignore
            }
        }

        // If running inside Docker, skip HTTPS certificate creation
        const runningInDocker = fs.existsSync('/.dockerenv') || fs.existsSync('/.dockerinit') || env.IN_DOCKER === 'true';
        if (runningInDocker) {
            console.warn("Detected Docker environment - skipping HTTPS certificate creation and running dev server without HTTPS.");
            useHttps = false;
        } else {
            // Check if cert files exist and are readable
            let certsExist = fs.existsSync(certFilePath) && fs.existsSync(keyFilePath);

            if (!certsExist) {
                // Check if dotnet is available before trying to create certificates
                let hasDotnet = false;
                try {
                    const res = child_process.spawnSync('dotnet', ['--version'], { stdio: 'ignore' });
                    hasDotnet = res && res.status === 0;
                } catch (e) {
                    hasDotnet = false;
                }

                if (hasDotnet) {
                    const r = child_process.spawnSync('dotnet', [
                        'dev-certs',
                        'https',
                        '--export-path',
                        certFilePath,
                        '--format',
                        'Pem',
                        '--no-password',
                    ], { stdio: 'inherit' });
                    if (r && r.status === 0) {
                        certsExist = fs.existsSync(certFilePath) && fs.existsSync(keyFilePath);
                    } else {
                        console.warn("Warning: Could not create certificate via 'dotnet dev-certs'. Falling back to non-HTTPS dev server.");
                        certsExist = false;
                    }
                } else {
                    console.warn("Warning: 'dotnet' not found. Skipping certificate creation and running Vite without HTTPS.");
                    certsExist = false;
                }
            }

            if (certsExist) {
                try {
                    fs.accessSync(certFilePath, fs.constants.R_OK);
                    fs.accessSync(keyFilePath, fs.constants.R_OK);
                    useHttps = true;
                } catch (e) {
                    console.warn("Warning: Certificate files are not readable. Falling back to non-HTTPS dev server.");
                    useHttps = false;
                }
            }
        }
    } catch (e) {
        console.warn('Unexpected error while preparing HTTPS certificate, falling back to non-HTTPS:', e && e.message ? e.message : e);
        useHttps = false;
    }

    // Read backend URLs from environment (for proxy targets in dev)
    // Use the correct ports: 7279 for auth, 5001 for API (see launchSettings.json)
    const authTarget = env.VITE_AUTH_API_BASE_URL || 'https://localhost:7279';
    const apiTarget = env.VITE_API_BASE_URL || 'https://localhost:5001';

    return {
        plugins: [plugin()],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        server: {
            proxy: {
                // Proxy auth requests to Authorization server
                '^/(login|register|.well-known)': {
                    target: authTarget,
                    changeOrigin: true,
                    secure: false
                },
                // Proxy API requests to main server
                '^/(api|Npcs)': {
                    target: apiTarget,
                    changeOrigin: true,
                    secure: false
                }
            },
            port: 49505,
            https: useHttps ? {
                key: fs.readFileSync(keyFilePath),
                cert: fs.readFileSync(certFilePath),
            } : false,
        }
    };
});
