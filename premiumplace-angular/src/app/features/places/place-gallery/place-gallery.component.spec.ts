import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PlaceGalleryComponent } from './place-gallery.component';

describe('PlaceGalleryComponent', () => {
  let component: PlaceGalleryComponent;
  let fixture: ComponentFixture<PlaceGalleryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PlaceGalleryComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PlaceGalleryComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('images', ['image-1.jpg', 'image-2.jpg']);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
