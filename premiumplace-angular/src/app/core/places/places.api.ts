import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import type { PlaceDto } from './places.models';

@Injectable({ providedIn: 'root' })
export class PlacesApi {
    private http = inject(HttpClient);

    list() {
        return this.http.get<PlaceDto[]>('/api/places');
    }

    getById(id: number) {
        return this.http.get<PlaceDto>(`/api/places/${id}`);
    }
}
