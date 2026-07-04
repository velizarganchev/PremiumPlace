import { ComponentFixture, TestBed } from '@angular/core/testing';
import { computed, signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { NavbarComponent } from './navbar.component';
import { AuthService } from '../../../core/auth/auth.service';
import type { User } from '../../../core/auth/auth.models';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let currentUser: ReturnType<typeof signal<User | null>>;
  let auth: jasmine.SpyObj<AuthService> & Pick<AuthService, 'user' | 'isLoggedIn' | 'isAdmin'>;

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

  it('shows username and logout for logged-in users', () => {
    currentUser.set({
      id: 2,
      username: 'demo',
      email: 'demo@premiumplace.local',
      role: 'User',
    });

    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;

    expect(text).toContain('demo');
    expect(text).toContain('Logout');
    expect(text).not.toContain('Login');
    expect(text).not.toContain('Register');
    expect(text).not.toContain('Admin');
  });

  it('shows an admin label for admin users', () => {
    currentUser.set({
      id: 1,
      username: 'admin',
      email: 'admin@premiumplace.local',
      role: 'Admin',
    });

    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('Admin');
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
});
