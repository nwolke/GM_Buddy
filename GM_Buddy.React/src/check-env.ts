#!/usr/bin/env node

/**
 * Environment Variable Checker
 * Run this to verify if Vite is loading your .env.local file
 */

console.log('\n=== Vite Environment Variables Check ===\n');

console.log('Expected values from .env.local:');
console.log('  VITE_USE_COGNITO: true');
console.log('  VITE_COGNITO_DOMAIN: us-west-23h6sioari.auth.us-west-2.amazoncognito.com');
console.log('  VITE_COGNITO_CLIENT_ID: 3tu41ptf62ntlqso884tl3aaem');

console.log('\nActual values loaded by Vite:');
console.log('  VITE_USE_COGNITO:', import.meta.env.VITE_USE_COGNITO);
console.log('  VITE_COGNITO_DOMAIN:', import.meta.env.VITE_COGNITO_DOMAIN || '(empty)');
console.log('  VITE_COGNITO_CLIENT_ID:', import.meta.env.VITE_COGNITO_CLIENT_ID ? `${import.meta.env.VITE_COGNITO_CLIENT_ID.substring(0, 10)}...` : '(empty)');

if (import.meta.env.VITE_USE_COGNITO === 'true') {
  console.log('\n? SUCCESS: Cognito is enabled');
} else {
  console.log('\n? PROBLEM: Cognito is NOT enabled');
  console.log('   Vite dev server needs to be restarted!');
}

console.log('\n========================================\n');
