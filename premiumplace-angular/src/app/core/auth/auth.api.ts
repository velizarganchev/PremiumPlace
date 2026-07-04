import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_PREFIX } from '../http/api.constants';
import type { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApi {
    private http = inject(HttpClient);

    register(body: RegisterRequest) {
        return this.http.post<AuthResponse>(`${API_PREFIX}/Auth/register`, body);
    }

    login(body: LoginRequest) {
        return this.http.post<AuthResponse>(`${API_PREFIX}/Auth/login`, body);
    }

    refresh() {
        return this.http.post<AuthResponse>(`${API_PREFIX}/Auth/refresh`, {});
    }

    logout() {
        return this.http.post<void>(`${API_PREFIX}/Auth/logout`, {});
    }

    me() {
        return this.http.get<AuthResponse>(`${API_PREFIX}/Auth/me`);
    }
}
