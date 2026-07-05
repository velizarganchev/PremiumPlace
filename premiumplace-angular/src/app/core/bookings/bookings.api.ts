import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { API_PREFIX } from '../http/api.constants';
import type {
    Availability,
    ConfirmBookingRequest,
    ConfirmBookingResult,
    CreateBookingRequest,
    CreatePendingResult,
    MyBooking,
    PaymentConfig,
} from './bookings.models';

@Injectable({ providedIn: 'root' })
export class BookingsApi {
    private http = inject(HttpClient);

    availability(placeId: number, from: string, to: string) {
        const params = new HttpParams()
            .set('placeId', placeId)
            .set('from', from)
            .set('to', to);

        return this.http.get<Availability>(`${API_PREFIX}/bookings/availability`, { params });
    }

    createPending(body: CreateBookingRequest) {
        return this.http.post<CreatePendingResult>(`${API_PREFIX}/bookings/pending`, body);
    }

    confirm(body: ConfirmBookingRequest) {
        return this.http.post<ConfirmBookingResult>(`${API_PREFIX}/bookings/confirm`, body);
    }

    myBookings() {
        return this.http.get<MyBooking[]>(`${API_PREFIX}/bookings/my`);
    }

    cancel(id: number) {
        return this.http.post<void>(`${API_PREFIX}/bookings/${id}/cancel`, {});
    }

    paymentConfig() {
        return this.http.get<PaymentConfig>(`${API_PREFIX}/payments/config`);
    }
}
