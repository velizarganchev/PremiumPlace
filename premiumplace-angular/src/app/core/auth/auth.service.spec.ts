import { TestBed } from '@angular/core/testing';
import { firstValueFrom, of, throwError } from 'rxjs';

import { AuthApi } from './auth.api';
import { AuthService } from './auth.service';
import type { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';

describe('AuthService', () => {
  const user: AuthResponse['user'] = {
    id: 1,
    username: 'admin',
    email: 'admin@premiumplace.local',
    role: 'Admin',
  };

  let api: jasmine.SpyObj<AuthApi>;
  let service: AuthService;

  beforeEach(() => {
    api = jasmine.createSpyObj<AuthApi>('AuthApi', ['me', 'login', 'register', 'logout', 'refresh']);

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: AuthApi, useValue: api },
      ],
    });

    service = TestBed.inject(AuthService);
  });

  it('loads the current user from the AuthResponse user property', async () => {
    api.me.and.returnValue(of({ user }));

    const result = await firstValueFrom(service.loadMe());

    expect(result).toEqual(user);
    expect(service.user()).toEqual(user);
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.isAdmin()).toBeTrue();
    expect(service.hasRole('Admin')).toBeTrue();
  });

  it('clears the session when loading the current user fails', async () => {
    api.login.and.returnValue(of({ user }));
    await firstValueFrom(service.login({ usernameOrEmail: 'admin', password: 'secret' } satisfies LoginRequest));
    expect(service.isLoggedIn()).toBeTrue();

    api.me.and.returnValue(throwError(() => new Error('Unauthorized')));

    const result = await firstValueFrom(service.loadMe());

    expect(result).toBeNull();
    expect(service.user()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('stores the user returned by login and register responses', async () => {
    api.login.and.returnValue(of({ user }));
    await firstValueFrom(service.login({ usernameOrEmail: 'admin', password: 'secret' } satisfies LoginRequest));
    expect(service.user()).toEqual(user);

    const registeredUser: AuthResponse['user'] = {
      id: 2,
      username: 'demo',
      email: 'demo@premiumplace.local',
      role: 'User',
    };

    api.register.and.returnValue(of({ user: registeredUser }));
    await firstValueFrom(service.register({
      username: 'demo',
      email: 'demo@premiumplace.local',
      password: 'secret',
    } satisfies RegisterRequest));

    expect(service.user()).toEqual(registeredUser);
    expect(service.isAdmin()).toBeFalse();
    expect(service.hasRole('User')).toBeTrue();
  });

  it('clears local auth state on logout and clearSession', async () => {
    api.login.and.returnValue(of({ user }));
    await firstValueFrom(service.login({ usernameOrEmail: 'admin', password: 'secret' } satisfies LoginRequest));

    api.logout.and.returnValue(of(void 0));
    await firstValueFrom(service.logout());
    expect(service.user()).toBeNull();

    api.login.and.returnValue(of({ user }));
    await firstValueFrom(service.login({ usernameOrEmail: 'admin', password: 'secret' } satisfies LoginRequest));
    service.clearSession();
    expect(service.user()).toBeNull();
  });
});
