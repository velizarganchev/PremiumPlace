import { Routes } from '@angular/router';
import { adminGuard, authGuard } from '../../core/guards/auth.guard';

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
                    import('../places/routes').then(m => m.PLACES_ROUTES),
            },
            {
                path: 'admin',
                canActivate: [adminGuard],
                loadComponent: () =>
                    import('../admin/admin-page/admin-page.component').then(m => m.AdminPageComponent),
            },
            {
                path: 'reservations',
                canActivate: [authGuard],
                loadComponent: () =>
                    import('../reservations/reservations.component').then(m => m.ReservationsComponent),
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
