import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

import { PlaceDetailsComponent } from './place-details.component';
import { PlacesService } from '../../../core/places/places.service';
import { AuthService } from '../../../core/auth/auth.service';

describe('PlaceDetailsComponent', () => {
  let component: PlaceDetailsComponent;
  let fixture: ComponentFixture<PlaceDetailsComponent>;
  let placesService: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'place' | 'loadingPlace'>;
  let placeState: ReturnType<typeof signal<any>>;
  let loggedIn: ReturnType<typeof signal<boolean>>;
  let router: Router;

  beforeEach(async () => {
    placeState = signal<any>(null);
    loggedIn = signal(false);

    placesService = {
      place: placeState.asReadonly(),
      loadingPlace: signal(false).asReadonly(),
      byId: jasmine.createSpy('byId').and.returnValue(of(null)),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'place' | 'loadingPlace'>;

    await TestBed.configureTestingModule({
      imports: [PlaceDetailsComponent],
      providers: [
        provideRouter([]),
        { provide: PlacesService, useValue: placesService },
        { provide: AuthService, useValue: { isLoggedIn: loggedIn.asReadonly() } },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(PlaceDetailsComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.componentRef.setInput('id', '1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads place details from the route id input', () => {
    expect(placesService.byId).toHaveBeenCalledWith(1);
  });

  it('redirects anonymous users to login instead of opening checkout', () => {
    const navigateByUrl = spyOn(router, 'navigateByUrl').and.resolveTo(true);

    component.onBook({ placeId: 1, start: new Date(2026, 7, 1), end: new Date(2026, 7, 4) });

    expect(navigateByUrl).toHaveBeenCalledWith('/auth/login');
    expect(component.booking()).toBeNull();
  });

  it('opens checkout with computed nights and total for logged-in users', () => {
    loggedIn.set(true);
    placeState.set({ rate: 100 });

    component.onBook({ placeId: 1, start: new Date(2026, 7, 1), end: new Date(2026, 7, 4) });

    const booking = component.booking();
    expect(booking).toEqual(jasmine.objectContaining({
      placeId: 1,
      checkInDate: '2026-08-01',
      checkOutDate: '2026-08-04',
      nights: 3,
      total: 300,
    }));
  });
});
