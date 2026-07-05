import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { PlacesService } from '../../../core/places/places.service';
import type { PagedResult, PlacePreview } from '../../../core/places/places.models';
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

  const paged = (total = 30): PagedResult<PlacePreview> => ({
    items: places,
    total,
    page: 1,
    pageSize: 12,
  });

  beforeEach(async () => {
    placesService = {
      places: signal(places).asReadonly(),
      loadingList: signal(false).asReadonly(),
      search: jasmine.createSpy('search').and.returnValue(of(paged())),
      cities: jasmine.createSpy('cities').and.returnValue(of(['Berlin', 'Munich'])),
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

  it('searches and loads the city facet on init', () => {
    expect(placesService.search).toHaveBeenCalled();
    expect(placesService.cities).toHaveBeenCalled();
    expect(component.total()).toBe(30);
    expect(component.cities()).toEqual(['Berlin', 'Munich']);
    expect(component.cards().length).toBe(1);
  });

  it('sends the selected city and resets to page 1', () => {
    component.page.set(3);

    component.onCityChange('Munich');

    const lastQuery = placesService.search.calls.mostRecent().args[0];
    expect(lastQuery).toEqual(jasmine.objectContaining({ city: 'Munich', page: 1 }));
  });

  it('debounces the search term', fakeAsync(() => {
    placesService.search.calls.reset();

    component.onQueryChange('loft');
    expect(placesService.search).not.toHaveBeenCalled();

    tick(300);

    expect(placesService.search).toHaveBeenCalledWith(
      jasmine.objectContaining({ search: 'loft', page: 1 }),
    );
  }));

  it('paginates to the next page', () => {
    component.onCityChange('all'); // total 30, pageSize 12 -> 3 pages
    placesService.search.calls.reset();

    component.nextPage();

    expect(component.page()).toBe(2);
    expect(placesService.search).toHaveBeenCalledWith(
      jasmine.objectContaining({ page: 2 }),
    );
  });

  it('sets the error signal when the search fails', () => {
    placesService.search.and.returnValue(throwError(() => new Error('Network down')));

    component.onSortChange('priceAsc');

    expect(component.error()).toBe('Network down');
  });
});
