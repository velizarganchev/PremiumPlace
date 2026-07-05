import { ComponentFixture, TestBed } from '@angular/core/testing';
import { computed, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';

import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../../core/auth/auth.service';
import type { User } from '../../../core/auth/auth.models';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let currentUser: ReturnType<typeof signal<User | null>>;
  let auth: jasmine.SpyObj<AuthService> & Pick<AuthService, 'user' | 'isLoggedIn' | 'isAdmin'>;
  let router: Router;

  beforeEach(async () => {
    currentUser = signal<User | null>(null);
    auth = {
      user: currentUser.asReadonly(),
      isLoggedIn: computed(() => currentUser() !== null),
      isAdmin: computed(() => currentUser()?.role === 'Admin'),
      logout: jasmine.createSpy('logout').and.returnValue(of(void 0)),
    } as unknown as jasmine.SpyObj<AuthService> & Pick<AuthService, 'user' | 'isLoggedIn' | 'isAdmin'>;

    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
      ],
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows public navigation for logged-out users', () => {
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;

    expect(text).toContain('PremiumPlace');
    expect(text).toContain('Places');
    expect(text).toContain('Login');
    expect(text).toContain('Register');
    expect(text).not.toContain('Logout');
    expect(text).not.toContain('My bookings');
    expect(text).not.toContain('Profile');
  });

  it('shows private navigation and logout for logged-in users', () => {
    currentUser.set({
      id: 2,
      username: 'demo',
      email: 'demo@premiumplace.local',
      role: 'User',
    });

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;

    expect(text).toContain('Places');
    expect(text).toContain('Logout');
    expect(text).not.toContain('Login');
    expect(text).not.toContain('Register');
    expect(text).not.toContain('Admin');
    expect(text).not.toContain('demo');
  });

  it('shows a clickable admin link for admin users', () => {
    currentUser.set({
      id: 1,
      username: 'admin',
      email: 'admin@premiumplace.local',
      role: 'Admin',
    });

    fixture.detectChanges();

    const adminLink = fixture.debugElement
      .queryAll(By.css('a'))
      .find(link => link.nativeElement.textContent.trim() === 'Admin');

    expect(adminLink).toBeTruthy();
    expect(adminLink!.attributes['routerLink']).toBe('/admin');
  });

  it('calls logout from the auth service', () => {
    currentUser.set({
      id: 2,
      username: 'demo',
      email: 'demo@premiumplace.local',
      role: 'User',
    });
    fixture.detectChanges();

    const logoutButton = fixture.debugElement
      .queryAll(By.css('button'))
      .find(button => button.nativeElement.textContent.includes('Logout'));

    logoutButton!.nativeElement.click();

    expect(auth.logout).toHaveBeenCalled();
  });

  it('redirects to login after logout from the admin page', () => {
    currentUser.set({
      id: 1,
      username: 'admin',
      email: 'admin@premiumplace.local',
      role: 'Admin',
    });
    spyOnProperty(router, 'url', 'get').and.returnValue('/admin');
    const navigateByUrl = spyOn(router, 'navigateByUrl').and.resolveTo(true);
    fixture.detectChanges();

    const logoutButton = fixture.debugElement
      .queryAll(By.css('button'))
      .find(button => button.nativeElement.textContent.includes('Logout'));

    logoutButton!.nativeElement.click();

    expect(auth.logout).toHaveBeenCalled();
    expect(navigateByUrl).toHaveBeenCalledWith('/auth/login');
  });
});
