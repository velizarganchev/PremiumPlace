import { CommonModule } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { PlacesService } from '../../../core/places/places.service';
import { mapPlaceToCard } from '../../../core/places/places.mapper';
import type { PlacePreview } from '../../../core/places/places.models';
import { CardsGridComponent } from '../../../shared/ui/cards-grid/cards-grid.component';

type SortKey = 'recommended' | 'priceAsc' | 'priceDesc' | 'capacityDesc';

@Component({
  selector: 'app-places-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    CardsGridComponent,
  ],
  templateUrl: './places-page.component.html',
  styleUrl: './places-page.component.scss'
})
export class PlacesPageComponent {
  private readonly placesService = inject(PlacesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly places = this.placesService.places;
  readonly loading = this.placesService.loadingList;

  readonly query = signal('');
  readonly city = signal('all');
  readonly sort = signal<SortKey>('recommended');
  readonly error = signal<string | null>(null);

  readonly cities = computed(() =>
    Array.from(new Set(this.places().map(place => place.city)))
      .filter(Boolean)
      .sort((a, b) => a.localeCompare(b))
  );

  readonly filteredPlaces = computed(() => {
    const query = this.query().trim().toLowerCase();
    const city = this.city();

    const filtered = this.places().filter(place => {
      const matchesCity = city === 'all' || place.city === city;
      const searchable = [
        place.name,
        place.city,
        place.details,
        ...(place.amenity ?? []),
      ].join(' ').toLowerCase();

      return matchesCity && (!query || searchable.includes(query));
    });

    return [...filtered].sort((a, b) => this.comparePlaces(a, b));
  });

  readonly cards = computed(() => this.filteredPlaces().map(mapPlaceToCard));

  ngOnInit() {
    this.placesService.loadAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: (err) => this.error.set(err?.message ?? 'Unable to load places.'),
      });
  }

  onQueryChange(value: string) {
    this.query.set(value);
  }

  onCityChange(value: string) {
    this.city.set(value);
  }

  onSortChange(value: SortKey) {
    this.sort.set(value);
  }

  clearFilters() {
    this.query.set('');
    this.city.set('all');
    this.sort.set('recommended');
  }

  private comparePlaces(a: PlacePreview, b: PlacePreview): number {
    switch (this.sort()) {
      case 'priceAsc':
        return a.rate - b.rate;
      case 'priceDesc':
        return b.rate - a.rate;
      case 'capacityDesc':
        return b.guestCapacity - a.guestCapacity;
      default:
        return a.id - b.id;
    }
  }
}
