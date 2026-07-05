import { inject, Injectable, signal, computed } from '@angular/core';
import { tap, map, finalize } from 'rxjs';
import { PlacesApi } from './places.api';
import { mapPlace, mapPlaceToCard } from './places.mapper';
import type { PlacePreview, PlaceDto, PlaceFormRequest } from './places.models';

@Injectable({ providedIn: 'root' })
export class PlacesService {
    private api = inject(PlacesApi);

    private _places = signal<PlacePreview[]>([]);
    private _loadingList = signal(false);

    private _place = signal<PlaceDto | null>(null);
    private _loadingPlace = signal(false);

    places = this._places.asReadonly();
    loadingList = this._loadingList.asReadonly();

    place = this._place.asReadonly();
    loadingPlace = this._loadingPlace.asReadonly();

    byId(id: number) {
        this._loadingPlace.set(true);
        return this.api.getById(id).pipe(
            tap(dto => this._place.set(dto)),
            finalize(() => this._loadingPlace.set(false))
        );
    }

    loadAll() {
        this._loadingList.set(true);

        return this.api.list().pipe(
            map(dtos => dtos.map(mapPlace)),
            tap({
                next: (places) => this._places.set(places),
                error: () => this._places.set([]),
            }),
            finalize(() => this._loadingList.set(false))
        );
    }

    options() {
        return this.api.options();
    }

    create(body: PlaceFormRequest) {
        return this.api.create(body).pipe(
            map(mapPlace),
            tap((place) => this._places.update(places => [...places, place]))
        );
    }

    update(id: number, body: PlaceFormRequest) {
        return this.api.update(id, body).pipe(
            map(mapPlace),
            tap((place) => this._places.update(places =>
                places.map(item => item.id === id ? place : item)
            ))
        );
    }

    delete(id: number) {
        return this.api.delete(id).pipe(
            tap(() => this._places.update(places => places.filter(place => place.id !== id)))
        );
    }

    cards = computed(() =>
        this.places().map(mapPlaceToCard)
    );

}
