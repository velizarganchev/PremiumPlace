import { ComponentFixture, fakeAsync, flushMicrotasks, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';

import { BookingCheckoutComponent } from './booking-checkout.component';
import { BookingsService } from '../../../../core/bookings/bookings.service';
import { PayPalLoaderService } from '../../../../core/payments/paypal-loader.service';

describe('BookingCheckoutComponent', () => {
  let fixture: ComponentFixture<BookingCheckoutComponent>;
  let component: BookingCheckoutComponent;
  let bookings: jasmine.SpyObj<BookingsService>;
  let loader: jasmine.SpyObj<PayPalLoaderService>;
  let buttonsSpy: jasmine.Spy;

  const setInputs = () => {
    fixture.componentRef.setInput('placeId', 1);
    fixture.componentRef.setInput('checkInDate', '2026-08-01');
    fixture.componentRef.setInput('checkOutDate', '2026-08-04');
    fixture.componentRef.setInput('nights', 3);
    fixture.componentRef.setInput('total', 360);
  };

  const getButtonOpts = () => buttonsSpy.calls.mostRecent().args[0];

  beforeEach(async () => {
    bookings = jasmine.createSpyObj<BookingsService>('BookingsService', ['createPending', 'confirm', 'paymentConfig']);
    loader = jasmine.createSpyObj<PayPalLoaderService>('PayPalLoaderService', ['load']);

    buttonsSpy = jasmine.createSpy('Buttons').and.returnValue({ render: () => {} });
    loader.load.and.returnValue(Promise.resolve({ Buttons: buttonsSpy }));

    bookings.createPending.and.returnValue(of({ bookingId: 9 }));
    bookings.paymentConfig.and.returnValue(of({ clientId: 'sandbox-id', currency: 'EUR' }));
    bookings.confirm.and.returnValue(of({ bookingId: 9, status: 'Confirmed' }));

    await TestBed.configureTestingModule({
      imports: [BookingCheckoutComponent],
      providers: [
        { provide: BookingsService, useValue: bookings },
        { provide: PayPalLoaderService, useValue: loader },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(BookingCheckoutComponent);
    component = fixture.componentInstance;
    setInputs();
  });

  it('renders PayPal buttons without creating a booking up front', fakeAsync(() => {
    fixture.detectChanges();
    flushMicrotasks();

    expect(loader.load).toHaveBeenCalledWith('sandbox-id', 'EUR');
    expect(buttonsSpy).toHaveBeenCalled();
    expect(bookings.createPending).not.toHaveBeenCalled(); // nothing recorded yet
    expect(component.status()).toBe('paying');
  }));

  it('creates the pending booking and order only when a PayPal button is used', fakeAsync(() => {
    fixture.detectChanges();
    flushMicrotasks();

    const create = jasmine.createSpy('create').and.resolveTo('PP-ORDER');
    getButtonOpts().createOrder(null, { order: { create } });
    flushMicrotasks();

    expect(bookings.createPending).toHaveBeenCalledWith({
      placeId: 1, checkInDate: '2026-08-01', checkOutDate: '2026-08-04',
    });
    expect(create).toHaveBeenCalledWith({
      purchase_units: [{ amount: { value: '360.00', currency_code: 'EUR' } }],
    });
  }));

  it('confirms the booking and emits on approval', fakeAsync(() => {
    let emitted: number | undefined;
    component.confirmed.subscribe(id => emitted = id);

    fixture.detectChanges();
    flushMicrotasks();

    const opts = getButtonOpts();
    opts.createOrder(null, { order: { create: jasmine.createSpy().and.resolveTo('PP') } });
    flushMicrotasks();

    opts.onApprove({ orderID: 'ORDER-1' });

    expect(bookings.confirm).toHaveBeenCalledWith({ bookingId: 9, paymentReference: 'ORDER-1' });
    expect(component.status()).toBe('done');
    expect(emitted).toBe(9);
  }));

  it('surfaces an error when the pending booking fails during payment', fakeAsync(() => {
    bookings.createPending.and.returnValue(throwError(() => new Error('No availability')));

    fixture.detectChanges();
    flushMicrotasks();

    const create = jasmine.createSpy('create');
    getButtonOpts().createOrder(null, { order: { create } }).catch(() => { /* PayPal aborts */ });
    flushMicrotasks();

    expect(component.status()).toBe('error');
    expect(component.error()).toBe('No availability');
    expect(create).not.toHaveBeenCalled();
  }));
});
