import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { PlaceDto, PlaceFormRequest, PlaceOptions } from './places.models';

@Injectable({ providedIn: 'root' })
export class PlacesApi {
    private http = inject(HttpClient);

    list() {
        return this.http.get<PlaceDto[]>('/api/places');
    }

    getById(id: number) {
        return this.http.get<PlaceDto>(`/api/places/${id}`);
    }

    options() {
        return this.http.get<PlaceOptions>('/api/places/options');
    }

    create(body: PlaceFormRequest) {
        return this.http.post<PlaceDto>('/api/places', body);
    }

    update(id: number, body: PlaceFormRequest) {
        return this.http.put<PlaceDto>(`/api/places/${id}`, { ...body, id });
    }

    delete(id: number) {
        return this.http.delete<void>(`/api/places/${id}`);
    }
}
