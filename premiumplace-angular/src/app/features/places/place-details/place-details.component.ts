import { CommonModule } from '@angular/common';
import { Component, computed, inject, input, signal } from '@angular/core';

import { Router, RouterModule } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { PlacesService } from '../../../core/places/places.service';
import { AuthService } from '../../../core/auth/auth.service';
import { PlaceGalleryComponent } from '../place-gallery/place-gallery.component';
import { PlaceBookingWidgetComponent } from './place-booking-widget/place-booking-widget.component';
import { BookingCheckoutComponent } from './booking-checkout/booking-checkout.component';

type BookingSelection = {
  placeId: number;
  checkInDate: string;
  checkOutDate: string;
  nights: number;
  total: number;
};

@Component({
  selector: 'app-place-details',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatDividerModule,
    PlaceGalleryComponent,
    PlaceBookingWidgetComponent,
    BookingCheckoutComponent,
  ],
  templateUrl: './place-details.component.html',
  styleUrl: './place-details.component.scss'
})
export class PlaceDetailsComponent {
  private placesService = inject(PlacesService);
  private auth = inject(AuthService);
  private router = inject(Router);

  readonly booking = signal<BookingSelection | null>(null);

  // route param
  id = input.required<string>();
  placeId = computed(() => Number(this.id()));

  ngOnInit() {
    this.placesService.byId(this.placeId()).subscribe();
  }

  place = this.placesService.place;
  loading = this.placesService.loadingPlace;

  stars = computed(() => {
    const avg = this.place()!.reviewSummary.avg;
    const full = Math.floor(avg);
    const half = avg - full >= 0.5;
    const empty = 5 - full - (half ? 1 : 0);
    return { full, half, empty };
  });

  gallery = computed(() => {
    const p = this.place();
    const base = p?.imageUrl ? [p.imageUrl] : [];
    return [...base, ...base, ...base].slice(0, 3);
  });

  showGallery = signal(false);

  onShowAllPhotos() {
    this.showGallery.set(true);
  }

  onCloseGallery() {
    this.showGallery.set(false);
  }

  /** Handles the booking widget's book event: require auth, then open checkout. */
  onBook(e: { placeId: number; start: Date; end: Date }) {
    if (!this.auth.isLoggedIn()) {
      this.router.navigateByUrl('/auth/login');
      return;
    }

    const nights = Math.round((e.end.getTime() - e.start.getTime()) / 86_400_000);
    if (nights < 1) return;

    const rate = this.place()?.rate ?? 0;

    this.booking.set({
      placeId: e.placeId,
      checkInDate: toIsoDate(e.start),
      checkOutDate: toIsoDate(e.end),
      nights,
      total: Math.round(rate * nights * 100) / 100,
    });
  }

  onCancelCheckout() {
    this.booking.set(null);
  }

  onBooked() {
    this.booking.set(null);
    this.router.navigateByUrl('/reservations');
  }
}

/** Formats a Date as a local yyyy-MM-dd string (no timezone shift). */
function toIsoDate(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}
