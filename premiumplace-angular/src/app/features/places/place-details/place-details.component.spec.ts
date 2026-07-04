import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { PlaceDetailsComponent } from './place-details.component';
import { PlacesService } from '../../../core/places/places.service';

describe('PlaceDetailsComponent', () => {
  let component: PlaceDetailsComponent;
  let fixture: ComponentFixture<PlaceDetailsComponent>;
  let placesService: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'place' | 'loadingPlace'>;

  beforeEach(async () => {
    placesService = {
      place: signal(null).asReadonly(),
      loadingPlace: signal(false).asReadonly(),
      byId: jasmine.createSpy('byId').and.returnValue(of(null)),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'place' | 'loadingPlace'>;

    await TestBed.configureTestingModule({
      imports: [PlaceDetailsComponent],
      providers: [
        provideRouter([]),
        { provide: PlacesService, useValue: placesService },
      ],
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PlaceDetailsComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('id', '1');
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads place details from the route id input', () => {
    expect(placesService.byId).toHaveBeenCalledWith(1);
  });
});
