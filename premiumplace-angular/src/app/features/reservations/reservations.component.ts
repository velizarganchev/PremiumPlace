import { CommonModule } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { BookingsService } from '../../core/bookings/bookings.service';
import type { MyBooking } from '../../core/bookings/bookings.models';

@Component({
  selector: 'app-reservations',
  standalone: true,
  imports: [CommonModule, RouterLink, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './reservations.component.html',
  styleUrl: './reservations.component.scss',
})
export class ReservationsComponent {
  private readonly bookings = inject(BookingsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly reservations = this.bookings.myBookings;
  readonly loading = this.bookings.loadingMyBookings;
  readonly error = signal<string | null>(null);

  ngOnInit() {
    this.bookings.loadMyBookings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: (err) => this.error.set(err?.message ?? 'Unable to load your reservations.'),
      });
  }

  canCancel(status: string): boolean {
    return status === 'Pending' || status === 'Confirmed';
  }

  statusClass(status: string): string {
    return `status status--${status.toLowerCase()}`;
  }

  cancel(booking: MyBooking) {
    this.error.set(null);
    this.bookings.cancel(booking.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: (err) => this.error.set(err?.message ?? 'Unable to cancel this booking.'),
      });
  }
}
