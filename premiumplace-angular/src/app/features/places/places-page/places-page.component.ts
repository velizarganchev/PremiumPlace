import { CommonModule } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { catchError, debounceTime, merge, of, Subject, switchMap, tap } from 'rxjs';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { PlacesService } from '../../../core/places/places.service';
import { mapPlaceToCard } from '../../../core/places/places.mapper';
import type { PlaceSortKey } from '../../../core/places/places.models';
import { CardsGridComponent } from '../../../shared/ui/cards-grid/cards-grid.component';

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
  readonly cards = computed(() => this.places().map(mapPlaceToCard));

  readonly query = signal('');
  readonly city = signal('all');
  readonly sort = signal<PlaceSortKey>('recommended');
  readonly page = signal(1);
  readonly pageSize = signal(12);
  readonly total = signal(0);
  readonly error = signal<string | null>(null);
  readonly cities = signal<string[]>([]);

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize())));
  readonly hasPrev = computed(() => this.page() > 1);
  readonly hasNext = computed(() => this.page() < this.totalPages());

  // Text input is debounced; selects and pagination fire immediately.
  private readonly immediate$ = new Subject<void>();
  private readonly debounced$ = new Subject<void>();

  constructor() {
    merge(this.immediate$, this.debounced$.pipe(debounceTime(300)))
      .pipe(
        switchMap(() => this.runSearch()),
        takeUntilDestroyed(),
      )
      .subscribe();
  }

  ngOnInit() {
    this.placesService.cities()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({ next: (cities) => this.cities.set(cities), error: () => { } });

    this.immediate$.next();
  }

  private runSearch() {
    this.error.set(null);

    return this.placesService.search({
      search: this.query().trim() || undefined,
      city: this.city() === 'all' ? undefined : this.city(),
      sort: this.sort(),
      page: this.page(),
      pageSize: this.pageSize(),
    }).pipe(
      tap((res) => this.total.set(res.total)),
      catchError((err) => {
        this.error.set(err?.message ?? 'Unable to load places.');
        return of(null);
      }),
    );
  }

  onQueryChange(value: string) {
    this.query.set(value);
    this.page.set(1);
    this.debounced$.next();
  }

  onCityChange(value: string) {
    this.city.set(value);
    this.page.set(1);
    this.immediate$.next();
  }

  onSortChange(value: PlaceSortKey) {
    this.sort.set(value);
    this.page.set(1);
    this.immediate$.next();
  }

  clearFilters() {
    this.query.set('');
    this.city.set('all');
    this.sort.set('recommended');
    this.page.set(1);
    this.immediate$.next();
  }

  prevPage() {
    if (!this.hasPrev()) return;
    this.page.update(p => p - 1);
    this.immediate$.next();
  }

  nextPage() {
    if (!this.hasNext()) return;
    this.page.update(p => p + 1);
    this.immediate$.next();
  }
}
