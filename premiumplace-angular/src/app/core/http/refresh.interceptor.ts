import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, concatMap, defer, from, throwError } from 'rxjs';
import { AuthApi } from '../auth/auth.api';
import { AuthService } from '../auth/auth.service';
import { API_PREFIX } from './api.constants';

let isRefreshing = false;
let refreshQueue: Array<() => void> = [];

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
    const authApi = inject(AuthApi);
    const authService = inject(AuthService);

    // only API calls
    if (!req.url.startsWith(API_PREFIX)) return next(req);

    // no refresh on auth endpoints (prevent loops)
    const isAuthEndpoint =
        req.url.endsWith('/Auth/login') ||
        req.url.endsWith('/Auth/register') ||
        req.url.endsWith('/Auth/refresh') ||
        req.url.endsWith('/Auth/logout') ||
        req.url.endsWith('/Auth/me');

    const retry = () => defer(() => next(req.clone()));

    return next(req).pipe(
        catchError((error: unknown) => {
            const is401 = error instanceof HttpErrorResponse && error.status === 401;
            if (!is401 || isAuthEndpoint) return throwError(() => error);

            // if refresh already running -> wait
            if (isRefreshing) {
                return from(new Promise<void>((resolve) => refreshQueue.push(resolve))).pipe(
                    concatMap(() => retry())
                );
            }

            isRefreshing = true;

            return authApi.refresh().pipe(
                concatMap(() => {
                    isRefreshing = false;
                    refreshQueue.forEach(r => r());
                    refreshQueue = [];
                    return retry();
                }),
                catchError((err) => {
                    isRefreshing = false;
                    refreshQueue = [];

                    authService.clearSession();

                    return throwError(() => err);
                })
            );
        })
    );
};
