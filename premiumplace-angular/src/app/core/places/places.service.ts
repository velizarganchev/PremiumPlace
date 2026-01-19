import { inject, Injectable, signal, computed } from '@angular/core';
import { tap, map } from 'rxjs';
import { PlacesApi } from './places.api';
import { mapPlace } from './places.mapper';
import type { Place } from './places.models';

@Injectable({ providedIn: 'root' })
export class PlacesService {
    private api = inject(PlacesApi);

    private _items = signal<Place[]>([]);
    private _loading = signal(false);

    items = this._items.asReadonly();
    loading = this._loading.asReadonly();

    byId = (id: number) => computed(() => this._items().find(p => p.id === id) ?? null);

    loadAll() {
        this._loading.set(true);

        return this.api.list().pipe(
            map(dtos => dtos.map(mapPlace)),
            tap({
                next: (places) => this._items.set(places),
                error: () => this._items.set([]),
            }),
            tap(() => this._loading.set(false))
        );
    }
}
