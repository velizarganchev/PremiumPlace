import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of } from 'rxjs';

import { BookingsService } from './bookings.service';
import { BookingsApi } from './bookings.api';
import type { MyBooking } from './bookings.models';

describe('BookingsService', () => {
  let service: BookingsService;
  let api: jasmine.SpyObj<BookingsApi>;

  const booking: MyBooking = {
    id: 5,
    placeId: 1,
    placeName: 'Berlin Loft',
    checkInDate: '2026-08-01',
    checkOutDate: '2026-08-04',
    status: 'Confirmed',
    createdAt: '2026-07-05T00:00:00Z',
    totalAmount: 360,
    currencyCode: 'EUR',
  };

  beforeEach(() => {
    api = jasmine.createSpyObj<BookingsApi>('BookingsApi', [
      'availability', 'createPending', 'confirm', 'myBookings', 'cancel', 'paymentConfig',
    ]);

    TestBed.configureTestingModule({
      providers: [
        BookingsService,
        { provide: BookingsApi, useValue: api },
      ],
    });

    service = TestBed.inject(BookingsService);
  });

  it('loads my bookings into state', async () => {
    api.myBookings.and.returnValue(of([booking]));

    await firstValueFrom(service.loadMyBookings());

    expect(service.myBookings()).toEqual([booking]);
    expect(service.loadingMyBookings()).toBeFalse();
  });

  it('marks a booking cancelled in state after cancel', async () => {
    api.myBookings.and.returnValue(of([booking]));
    api.cancel.and.returnValue(of(void 0));
    await firstValueFrom(service.loadMyBookings());

    await firstValueFrom(service.cancel(5));

    expect(service.myBookings()[0].status).toBe('Cancelled');
    expect(api.cancel).toHaveBeenCalledWith(5);
  });

  it('delegates createPending and confirm to the api', () => {
    api.createPending.and.returnValue(of({ bookingId: 9 }));
    api.confirm.and.returnValue(of({ bookingId: 9, status: 'Confirmed' }));

    service.createPending({ placeId: 1, checkInDate: '2026-08-01', checkOutDate: '2026-08-04' });
    service.confirm({ bookingId: 9, paymentReference: 'ORDER-1' });

    expect(api.createPending).toHaveBeenCalled();
    expect(api.confirm).toHaveBeenCalledWith({ bookingId: 9, paymentReference: 'ORDER-1' });
  });
});
