import { test, expect } from '../fixtures/test-fixtures';

test.describe('API Endpoint Smoke Tests', () => {

    test('GET /api/joke returns 200 with valid JSON', async ({ request }) => {
        const response = await request.get('/api/joke', {
            headers: { 'ApiKey': 'DadJokes!' },
        });
        expect(response.status()).toBe(200);
        expect(response.ok()).toBeTruthy();
        const body = await response.json();
        expect(body).toBeTruthy();
    });

    test('GET /api/joke/category/Chickens returns 200', async ({ request }) => {
        const response = await request.get('/api/joke/category/Chickens', {
            headers: { 'ApiKey': 'DadJokes!' },
        });
        expect(response.status()).toBe(200);
        expect(response.ok()).toBeTruthy();
        const body = await response.json();
        expect(body).toBeTruthy();
    });

    test('API rejects requests without ApiKey header', async ({ request }) => {
        const response = await request.get('/api/joke', {
            headers: {},
        });
        // Should return an error status without the API key
        expect(response.ok()).toBeFalsy();
    });
});
