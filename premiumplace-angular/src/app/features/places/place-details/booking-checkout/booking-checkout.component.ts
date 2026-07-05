import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  inject,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { BookingsService } from '../../../../core/bookings/bookings.service';
import { PayPalLoaderService } from '../../../../core/payments/paypal-loader.service';

type CheckoutStatus = 'loading' | 'paying' | 'creating' | 'confirming' | 'done' | 'error';

@Component({
  selector: 'app-booking-checkout',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './booking-checkout.component.html',
  styleUrl: './booking-checkout.component.scss',
})
export class BookingCheckoutComponent {
  private readonly bookings = inject(BookingsService);
  private readonly loader = inject(PayPalLoaderService);

  // yyyy-MM-dd strings matching the backend DateOnly.
  readonly placeId = input.required<number>();
  readonly checkInDate = input.required<string>();
  readonly checkOutDate = input.required<string>();
  readonly nights = input.required<number>();
  readonly total = input.required<number>();
  readonly currency = input<string>('EUR');

  readonly confirmed = output<number>();
  readonly cancelled = output<void>();

  readonly status = signal<CheckoutStatus>('loading');
  readonly error = signal<string | null>(null);

  private readonly paypalHost = viewChild<ElementRef<HTMLElement>>('paypal');
  private bookingId?: number;

  ngOnInit() {
    // No booking is created up front. It is created only when the user actually
    // starts paying (PayPal createOrder), so merely opening checkout records nothing.
    void this.renderPayPal();
  }

  cancel() {
    this.cancelled.emit();
  }

  private async renderPayPal() {
    try {
      this.status.set('loading');
      this.error.set(null);

      const config = await firstValueFrom(this.bookings.paymentConfig());
      const currency = config.currency || this.currency();
      const paypal = await this.loader.load(config.clientId, currency);

      const host = this.paypalHost()?.nativeElement;
      if (!host) return;
      host.innerHTML = '';

      paypal.Buttons({
        // Create the pending booking only now, when the user commits to paying.
        createOrder: async (_data: unknown, actions: any) => {
          try {
            this.status.set('creating');
            const res = await firstValueFrom(this.bookings.createPending({
              placeId: this.placeId(),
              checkInDate: this.checkInDate(),
              checkOutDate: this.checkOutDate(),
            }));
            this.bookingId = res.bookingId;
            this.status.set('paying');

            return await actions.order.create({
              purchase_units: [{
                amount: { value: this.total().toFixed(2), currency_code: currency },
              }],
            });
          } catch (err: any) {
            this.fail(err?.message ?? 'Could not reserve your dates.');
            throw err;
          }
        },
        onApprove: (data: { orderID: string }) => this.confirm(data.orderID),
        onError: () => this.fail('Payment could not be completed.'),
        onCancel: () => this.status.set('paying'),
      }).render(host);

      this.status.set('paying');
    } catch (err: any) {
      this.fail(err?.message ?? 'Payment is unavailable right now.');
    }
  }

  private confirm(orderId: string) {
    if (!this.bookingId) {
      this.fail('Missing booking reference.');
      return;
    }

    this.status.set('confirming');

    this.bookings.confirm({
      bookingId: this.bookingId,
      paymentReference: orderId,
    }).subscribe({
      next: () => {
        this.status.set('done');
        this.confirmed.emit(this.bookingId!);
      },
      error: (err) => this.fail(err?.message ?? 'Payment verification failed.'),
    });
  }

  private fail(message: string) {
    this.error.set(message);
    this.status.set('error');
  }
}
