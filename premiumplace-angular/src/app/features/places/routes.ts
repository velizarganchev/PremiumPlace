import { Routes } from '@angular/router';

export const PLACES_ROUTES: Routes = [
    { path: '', loadComponent: () => import('./places-page/places-page.component').then(m => m.PlacesPageComponent) },
    { path: ':id', loadComponent: () => import('./place-details/place-details.component').then(m => m.PlaceDetailsComponent) },
];
