import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { PlacesService } from '../../../core/places/places.service';
import type { PlacePreview } from '../../../core/places/places.models';
import { PlacesPageComponent } from './places-page.component';

describe('PlacesPageComponent', () => {
  let component: PlacesPageComponent;
  let fixture: ComponentFixture<PlacesPageComponent>;
  let placesService: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places' | 'loadingList'>;

  const places: PlacePreview[] = [
    {
      id: 1,
      name: 'Berlin Loft',
      details: 'Central city stay',
      city: 'Berlin',
      cityId: 1,
      rate: 120,
      imageUrl: 'image-1.jpg',
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
      reviewSummary: { avg: 4.8, count: 4 },
    },
  ];

  beforeEach(async () => {
    placesService = {
      places: signal(places).asReadonly(),
      loadingList: signal(false).asReadonly(),
      loadAll: jasmine.createSpy('loadAll').and.returnValue(of(places)),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places' | 'loadingList'>;

    await TestBed.configureTestingModule({
      imports: [PlacesPageComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        { provide: PlacesService, useValue: placesService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PlacesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads places and filters by query', () => {
    expect(placesService.loadAll).toHaveBeenCalled();

    component.onQueryChange('loft');

    expect(component.filteredPlaces().length).toBe(1);
  });
});
