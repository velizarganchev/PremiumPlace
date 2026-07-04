import { inject, Injectable, signal, computed } from '@angular/core';
import { catchError, map, of, tap } from 'rxjs';
import { AuthApi } from './auth.api';
import type { LoginRequest, RegisterRequest, User, UserRole } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private api = inject(AuthApi);

    private _user = signal<User | null>(null);
    user = this._user.asReadonly();

    isLoggedIn = computed(() => this._user() !== null);
    isAdmin = computed(() => this._user()?.role === 'Admin');

    // Load session from cookies
    loadMe() {
        return this.api.me().pipe(
            map((response) => response.user),
            tap((user) => this._user.set(user)),
            catchError(() => {
                this.clearSession();
                return of(null);
            })
        );
    }

    login(body: LoginRequest) {
        return this.api.login(body).pipe(
            tap((response) => this._user.set(response.user)),
            map(() => void 0)
        );
    }

    register(body: RegisterRequest) {
        return this.api.register(body).pipe(
            tap((response) => this._user.set(response.user)),
            map(() => void 0)
        );
    }

    logout() {
        return this.api.logout().pipe(
            tap(() => this._user.set(null)),
            map(() => void 0)
        );
    }

    clearSession() {
        this._user.set(null);
    }

    hasRole(role: UserRole) {
        return this._user()?.role === role;
    }
}
