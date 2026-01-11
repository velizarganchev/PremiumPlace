import { Routes } from '@angular/router';

export const routes: Routes = [

    {
        path: '',
        loadChildren: () =>
            import('./features/shell/routes').then(m => m.SHELL_ROUTES),
    },

    {
        path: 'auth',
        loadChildren: () =>
            import('./features/auth/routes').then(m => m.AUTH_ROUTES),
    },

    // fallback
    { path: '**', redirectTo: '' },
];
