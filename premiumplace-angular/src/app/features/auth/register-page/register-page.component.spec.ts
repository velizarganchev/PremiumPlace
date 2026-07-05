import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';

import { RegisterPageComponent } from './register-page.component';
import { AuthService } from '../../../core/auth/auth.service';

describe('RegisterPageComponent', () => {
  let component: RegisterPageComponent;
  let fixture: ComponentFixture<RegisterPageComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  const fillValidForm = () => {
    component.form.setValue({
      username: 'newuser',
      email: 'new@premiumplace.local',
      password: 'secret1',
      confirmPassword: 'secret1',
    });
  };

  beforeEach(async () => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['register']);
    auth.register.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [RegisterPageComponent],
      providers: [
        provideRouter([]),
        provideNoopAnimations(),
        { provide: AuthService, useValue: auth },
      ],
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterPageComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not call register when the form is empty/invalid', () => {
    component.onSubmit();

    expect(auth.register).not.toHaveBeenCalled();
    expect(component.form.touched).toBeTrue();
  });

  it('flags a password mismatch and blocks submission', () => {
    component.form.setValue({
      username: 'newuser',
      email: 'new@premiumplace.local',
      password: 'secret1',
      confirmPassword: 'secret2',
    });

    component.onSubmit();

    expect(component.form.get('confirmPassword')!.hasError('mismatch')).toBeTrue();
    expect(auth.register).not.toHaveBeenCalled();
  });

  it('registers and navigates home on a valid submission', () => {
    const navigateByUrl = spyOn(router, 'navigateByUrl').and.resolveTo(true);
    fillValidForm();

    component.onSubmit();

    expect(auth.register).toHaveBeenCalledWith({
      username: 'newuser',
      email: 'new@premiumplace.local',
      password: 'secret1',
    });
    expect(navigateByUrl).toHaveBeenCalledWith('/');
    expect(component.loading()).toBeFalse();
    expect(component.error()).toBeNull();
  });

  it('surfaces the error and clears loading when register fails', () => {
    auth.register.and.returnValue(throwError(() => new Error('Email already in use')));
    fillValidForm();

    component.onSubmit();

    expect(component.error()).toBe('Email already in use');
    expect(component.loading()).toBeFalse();
  });

  it('ignores a second submit while a request is in flight', () => {
    component.loading.set(true);
    fillValidForm();

    component.onSubmit();

    expect(auth.register).not.toHaveBeenCalled();
  });
});
