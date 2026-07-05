import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { PlacesService } from '../../../core/places/places.service';
import type { PlaceOptions, PlacePreview } from '../../../core/places/places.models';
import { AdminPageComponent } from './admin-page.component';

describe('AdminPageComponent', () => {
  let component: AdminPageComponent;
  let fixture: ComponentFixture<AdminPageComponent>;
  let placesState: ReturnType<typeof signal<PlacePreview[]>>;
  let service: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places' | 'loadingList'>;

  const options: PlaceOptions = {
    cities: [{ id: 1, name: 'Berlin' }],
    amenities: [{ id: 1, name: 'Free Wi-Fi' }],
  };

  const place: PlacePreview = {
    id: 7,
    name: 'Admin Place',
    details: 'Central stay',
    city: 'Berlin',
    cityId: 1,
    rate: 120,
    imageUrl: 'https://example.com/place.jpg',
    amenity: ['Free Wi-Fi'],
    amenityIds: [1],
    features: {
      internet: true,
      airConditioned: false,
      petsAllowed: false,
      parking: false,
      entertainment: false,
      kitchen: true,
      refrigerator: false,
      washer: false,
      dryer: false,
      selfCheckIn: false,
    },
    guestCapacity: 2,
    beds: 1,
    reviewSummary: { avg: 5, count: 3 },
  };

  beforeEach(async () => {
    placesState = signal<PlacePreview[]>([place]);
    const loadingState = signal(false);

    service = {
      places: placesState.asReadonly(),
      loadingList: loadingState.asReadonly(),
      loadAll: jasmine.createSpy('loadAll').and.returnValue(of([place])),
      options: jasmine.createSpy('options').and.returnValue(of(options)),
      byId: jasmine.createSpy('byId').and.returnValue(of({
        ...place,
        checkInHour: 14,
        checkOutHour: 11,
        squareFeet: 500,
        amenitys: place.amenity,
        reviews: [],
      })),
      create: jasmine.createSpy('create').and.returnValue(of(place)),
      update: jasmine.createSpy('update').and.returnValue(of(place)),
      delete: jasmine.createSpy('delete').and.returnValue(of(void 0)),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places' | 'loadingList'>;

    await TestBed.configureTestingModule({
      imports: [AdminPageComponent],
      providers: [
        provideNoopAnimations(),
        { provide: PlacesService, useValue: service },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads places and form options on init', () => {
    expect(service.loadAll).toHaveBeenCalled();
    expect(service.options).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Places management');
    expect(fixture.nativeElement.textContent).toContain('Admin Place');
  });

  it('patches the form when editing a place', () => {
    component.editPlace(place);

    expect(service.byId).toHaveBeenCalledWith(7);
    expect(component.form.controls.name.value).toBe('Admin Place');
    expect(component.form.controls.cityId.value).toBe(1);
    expect(component.hasAmenity(1)).toBeTrue();
  });

  it('keeps amenities checked on edit when details omit amenityIds', () => {
    // Reproduce the reported bug: details payload without amenityIds must not clear the selection.
    service.byId.and.returnValue(of({
      ...place,
      checkInHour: 14,
      checkOutHour: 11,
      squareFeet: 500,
      amenitys: place.amenity,
      amenityIds: [],
      reviews: [],
    }));

    component.editPlace(place); // place.amenityIds === [1]

    expect(component.hasAmenity(1)).toBeTrue();
    expect(component.form.controls.amenityIds.value).toEqual([1]);
  });

  it('creates a new place with the selected existing city', () => {
    component.startCreate();
    component.form.controls.name.setValue('New Loft');

    component.save();

    expect(service.create).toHaveBeenCalledWith(
      jasmine.objectContaining({ name: 'New Loft', cityId: 1, cityName: null }),
    );
    expect(service.update).not.toHaveBeenCalled();
  });

  it('sends cityName when adding a new city', () => {
    component.startCreate();
    component.form.controls.name.setValue('Dresden Loft');
    component.form.controls.cityId.setValue(0);
    component.form.controls.cityName.setValue('  Dresden ');

    component.save();

    expect(service.create).toHaveBeenCalledWith(
      jasmine.objectContaining({ cityId: 0, cityName: 'Dresden' }),
    );
  });

  it('blocks saving a new city with no name', () => {
    component.startCreate();
    component.form.controls.name.setValue('No City Place');
    component.form.controls.cityId.setValue(0);
    component.form.controls.cityName.setValue('');

    component.save();

    expect(service.create).not.toHaveBeenCalled();
    expect(component.form.controls.cityName.hasError('required')).toBeTrue();
  });

  it('updates the selected place', () => {
    component.editPlace(place);
    component.form.controls.rate.setValue(999);

    component.save();

    expect(service.update).toHaveBeenCalledWith(
      7,
      jasmine.objectContaining({ id: 7, rate: 999 }),
    );
    expect(service.create).not.toHaveBeenCalled();
  });

  it('deletes a place after confirmation', () => {
    component.deletePlace(place);
    expect(component.placePendingDelete()).toBe(place);

    component.confirmDelete();

    expect(service.delete).toHaveBeenCalledWith(7);
    expect(component.placePendingDelete()).toBeNull();
  });

  it('surfaces an error when saving fails', () => {
    service.create.and.returnValue(throwError(() => new Error('Save failed')));
    component.startCreate();
    component.form.controls.name.setValue('Broken');

    component.save();

    expect(component.error()).toBe('Save failed');
    expect(component.saving()).toBeFalse();
  });
});
