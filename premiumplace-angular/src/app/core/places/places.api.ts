import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import type { PagedResult, PlaceDto, PlaceFormRequest, PlaceOptions, PlaceQuery } from './places.models';

@Injectable({ providedIn: 'root' })
export class PlacesApi {
    private http = inject(HttpClient);

    list() {
        return this.http.get<PlaceDto[]>('/api/places');
    }

    search(query: PlaceQuery) {
        let params = new HttpParams();
        if (query.search) params = params.set('search', query.search);
        if (query.city) params = params.set('city', query.city);
        if (query.sort) params = params.set('sort', query.sort);
        if (query.page) params = params.set('page', query.page);
        if (query.pageSize) params = params.set('pageSize', query.pageSize);

        return this.http.get<PagedResult<PlaceDto>>('/api/places/search', { params });
    }

    cities() {
        return this.http.get<string[]>('/api/places/cities');
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
