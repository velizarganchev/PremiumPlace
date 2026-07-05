import { inject, Injectable, signal } from '@angular/core';
import { finalize, tap } from 'rxjs';

import { BookingsApi } from './bookings.api';
import type {
    ConfirmBookingRequest,
    CreateBookingRequest,
    MyBooking,
} from './bookings.models';

@Injectable({ providedIn: 'root' })
export class BookingsService {
    private api = inject(BookingsApi);

    private readonly _myBookings = signal<MyBooking[]>([]);
    private readonly _loadingMine = signal(false);

    readonly myBookings = this._myBookings.asReadonly();
    readonly loadingMyBookings = this._loadingMine.asReadonly();

    availability(placeId: number, from: string, to: string) {
        return this.api.availability(placeId, from, to);
    }

    createPending(body: CreateBookingRequest) {
        return this.api.createPending(body);
    }

    confirm(body: ConfirmBookingRequest) {
        return this.api.confirm(body);
    }

    paymentConfig() {
        return this.api.paymentConfig();
    }

    loadMyBookings() {
        this._loadingMine.set(true);

        return this.api.myBookings().pipe(
            tap({
                next: (bookings) => this._myBookings.set(bookings),
                error: () => this._myBookings.set([]),
            }),
            finalize(() => this._loadingMine.set(false)),
        );
    }

    cancel(id: number) {
        return this.api.cancel(id).pipe(
            tap(() => this._myBookings.update(list =>
                list.map(b => b.id === id ? { ...b, status: 'Cancelled' } : b),
            )),
        );
    }
}
