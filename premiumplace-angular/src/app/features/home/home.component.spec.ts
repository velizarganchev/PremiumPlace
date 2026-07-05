import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { HomeComponent } from './home.component';
import { PlacesService } from '../../core/places/places.service';
import type { PlacePreview } from '../../core/places/places.models';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let placesService: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places'>;

  const place: PlacePreview = {
    id: 1,
    name: 'Top Place',
    details: 'Nice stay',
    city: 'Berlin',
    cityId: 1,
    rate: 100,
    imageUrl: 'image-1.jpg',
    amenity: [],
    amenityIds: [],
    features: {
      internet: true,
      airConditioned: false,
      petsAllowed: false,
      parking: false,
      entertainment: false,
      kitchen: false,
      refrigerator: false,
      washer: false,
      dryer: false,
      selfCheckIn: false,
    },
    guestCapacity: 2,
    beds: 1,
    reviewSummary: { avg: 5, count: 2 },
  };

  beforeEach(async () => {
    placesService = {
      places: signal<PlacePreview[]>([place]).asReadonly(),
      loadAll: jasmine.createSpy('loadAll').and.returnValue(of([place])),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'places'>;

    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        provideRouter([]),
        { provide: PlacesService, useValue: placesService },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads places on init', () => {
    expect(placesService.loadAll).toHaveBeenCalled();
  });
});
