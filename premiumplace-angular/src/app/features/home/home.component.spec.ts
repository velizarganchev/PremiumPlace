import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { HomeComponent } from './home.component';
import { PlacesService } from '../../core/places/places.service';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let placesService: jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'cards'>;

  beforeEach(async () => {
    placesService = {
      cards: signal([]).asReadonly(),
      loadAll: jasmine.createSpy('loadAll').and.returnValue(of([])),
    } as unknown as jasmine.SpyObj<PlacesService> & Pick<PlacesService, 'cards'>;

    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        provideRouter([]),
        { provide: PlacesService, useValue: placesService },
      ],
    })
    .compileComponents();
    
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
