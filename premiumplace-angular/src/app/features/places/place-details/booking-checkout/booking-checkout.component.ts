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

type CheckoutStatus = 'idle' | 'creating' | 'paying' | 'confirming' | 'done' | 'error';

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

  readonly status = signal<CheckoutStatus>('idle');
  readonly error = signal<string | null>(null);

  private readonly paypalHost = viewChild<ElementRef<HTMLElement>>('paypal');
  private bookingId?: number;

  ngOnInit() {
    this.start();
  }

  /** Creates the pending booking, then renders the PayPal buttons. */
  start() {
    this.status.set('creating');
    this.error.set(null);

    this.bookings.createPending({
      placeId: this.placeId(),
      checkInDate: this.checkInDate(),
      checkOutDate: this.checkOutDate(),
    }).subscribe({
      next: (res) => {
        this.bookingId = res.bookingId;
        void this.renderPayPal();
      },
      error: (err) => this.fail(err?.message ?? 'Could not start the booking.'),
    });
  }

  cancel() {
    this.cancelled.emit();
  }

  private async renderPayPal() {
    try {
      this.status.set('paying');

      const config = await firstValueFrom(this.bookings.paymentConfig());
      const currency = config.currency || this.currency();
      const paypal = await this.loader.load(config.clientId, currency);

      const host = this.paypalHost()?.nativeElement;
      if (!host) return;
      host.innerHTML = '';

      paypal.Buttons({
        createOrder: (_data: unknown, actions: any) => actions.order.create({
          purchase_units: [{
            amount: { value: this.total().toFixed(2), currency_code: currency },
          }],
        }),
        onApprove: (data: { orderID: string }) => this.confirm(data.orderID),
        onError: () => this.fail('Payment could not be completed.'),
        onCancel: () => this.status.set('idle'),
      }).render(host);
    } catch (err: any) {
      this.fail(err?.message ?? 'Payment is unavailable right now.');
    }
  }

  private confirm(orderId: string) {
    this.status.set('confirming');

    this.bookings.confirm({
      bookingId: this.bookingId!,
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
