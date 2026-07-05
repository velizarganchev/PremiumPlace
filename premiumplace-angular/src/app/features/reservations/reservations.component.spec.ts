import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ReservationsComponent } from './reservations.component';
import { BookingsService } from '../../core/bookings/bookings.service';
import type { MyBooking } from '../../core/bookings/bookings.models';

describe('ReservationsComponent', () => {
  let component: ReservationsComponent;
  let fixture: ComponentFixture<ReservationsComponent>;
  let bookings: jasmine.SpyObj<BookingsService> & Pick<BookingsService, 'myBookings' | 'loadingMyBookings'>;

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

  beforeEach(async () => {
    bookings = {
      myBookings: signal<MyBooking[]>([booking]).asReadonly(),
      loadingMyBookings: signal(false).asReadonly(),
      loadMyBookings: jasmine.createSpy('loadMyBookings').and.returnValue(of([booking])),
      cancel: jasmine.createSpy('cancel').and.returnValue(of(void 0)),
    } as unknown as jasmine.SpyObj<BookingsService> & Pick<BookingsService, 'myBookings' | 'loadingMyBookings'>;

    await TestBed.configureTestingModule({
      imports: [ReservationsComponent],
      providers: [
        provideRouter([]),
        { provide: BookingsService, useValue: bookings },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ReservationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads reservations on init and renders them', () => {
    expect(bookings.loadMyBookings).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Berlin Loft');
    expect(fixture.nativeElement.textContent).toContain('EUR 360.00');
  });

  it('allows cancelling active bookings only', () => {
    expect(component.canCancel('Confirmed')).toBeTrue();
    expect(component.canCancel('Pending')).toBeTrue();
    expect(component.canCancel('Cancelled')).toBeFalse();
    expect(component.canCancel('Expired')).toBeFalse();
  });

  it('cancels a booking through the service', () => {
    component.cancel(booking);
    expect(bookings.cancel).toHaveBeenCalledWith(5);
  });
});
