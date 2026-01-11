import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards/auth.guard';

export const SHELL_ROUTES: Routes = [
    {
        path: '',
        loadComponent: () =>
            import('./shell-layout/shell-layout.component').then(m => m.ShellLayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () =>
                    import('../home/home.component').then(m => m.HomeComponent),
            },

            {
                path: 'places',
                loadChildren: () =>
                    import('../places/places-page/routes').then(m => m.PLACES_ROUTES),
            },
            // Booking (PRIVATE)
            // {
            //     path: 'booking',
            //     canActivate: [authGuard],
            //     loadChildren: () =>
            //         import('../booking/routes').then(m => m.BOOKING_ROUTES),
            // },
        ],
    },
];
