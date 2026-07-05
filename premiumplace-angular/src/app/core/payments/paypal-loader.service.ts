import { Injectable } from '@angular/core';

/**
 * Loads the PayPal JS SDK once and hands back the global `paypal` object.
 * The client id is a public sandbox id fetched from the backend config.
 */
@Injectable({ providedIn: 'root' })
export class PayPalLoaderService {
    private loading?: Promise<any>;

    load(clientId: string, currency: string): Promise<any> {
        if (this.loading) return this.loading;

        this.loading = new Promise<any>((resolve, reject) => {
            const existing = (window as any).paypal;
            if (existing) {
                resolve(existing);
                return;
            }

            if (!clientId) {
                reject(new Error('PayPal client id is not configured.'));
                return;
            }

            const script = document.createElement('script');
            script.src =
                `https://www.paypal.com/sdk/js?client-id=${encodeURIComponent(clientId)}` +
                `&currency=${encodeURIComponent(currency)}&intent=capture`;
            script.onload = () => {
                const paypal = (window as any).paypal;
                paypal ? resolve(paypal) : reject(new Error('PayPal SDK loaded without a global.'));
            };
            script.onerror = () => reject(new Error('Failed to load the PayPal SDK.'));
            document.body.appendChild(script);
        });

        return this.loading;
    }
}
