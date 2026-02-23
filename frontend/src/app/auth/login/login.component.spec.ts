// T070: Tests for LoginComponent (Login + Register modes)
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { Component } from '@angular/core';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../auth.service';

@Component({ standalone: true, template: '' })
class DummyChatComponent {}

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    authService = jasmine.createSpyObj('AuthService', ['login', 'register']);

    await TestBed.configureTestingModule({
      imports: [
        LoginComponent,
        RouterTestingModule.withRoutes([
          { path: 'chat', component: DummyChatComponent },
          { path: 'login', component: LoginComponent }
        ])
      ],
      providers: [{ provide: AuthService, useValue: authService }]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Login Mode', () => {
    beforeEach(() => {
      component.isRegistering = false;
      fixture.detectChanges();
    });

    it('should render email and password inputs', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.querySelector('input[type="email"]')).toBeTruthy();
      expect(compiled.querySelector('input[type="password"]')).toBeTruthy();
    });

    it('should show "Se connecter" heading', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Connexion');
    });

    it('should not call login when fields are empty', () => {
      component.email = '';
      component.password = '';
      component.onSubmit();
      expect(component.error).toBeTruthy();
      expect(authService.login).not.toHaveBeenCalled();
    });

    it('should call AuthService.login with email and password', () => {
      authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'test@example.com' }));
      component.email = 'test@example.com';
      component.password = 'secret123';
      component.onSubmit();
      expect(authService.login).toHaveBeenCalledWith('test@example.com', 'secret123');
    });

    it('should navigate to /chat on successful login', fakeAsync(() => {
      authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'test@example.com' }));
      const navSpy = spyOn(router, 'navigate');
      component.email = 'test@example.com';
      component.password = 'secret123';
      component.onSubmit();
      tick();
      expect(navSpy).toHaveBeenCalledWith(['/chat']);
    }));

    it('should display error message on login failure', fakeAsync(() => {
      authService.login.and.returnValue(throwError(() => ({ error: { message: 'Invalid credentials' } })));
      component.email = 'test@example.com';
      component.password = 'wrongpass';
      component.onSubmit();
      tick();
      expect(component.error).toContain('Invalid credentials');
      expect(component.loading).toBeFalse();
    }));
  });

  describe('Register Mode', () => {
    beforeEach(() => {
      component.isRegistering = true;
      fixture.detectChanges();
    });

    it('should show "Créer un compte" heading', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      expect(compiled.textContent).toContain('Créer un compte');
    });

    it('should render password confirm input in register mode', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const passwordInputs = compiled.querySelectorAll('input[type="password"]');
      expect(passwordInputs.length).toBe(2); // password + confirm
    });

    it('should show error when passwords do not match', () => {
      component.email = 'new@example.com';
      component.password = 'SecurePass123!';
      component.passwordConfirm = 'Different123!';
      component.onSubmit();
      expect(component.error).toContain('ne correspondent pas');
      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should show error when password is too short', () => {
      component.email = 'new@example.com';
      component.password = 'short';
      component.passwordConfirm = 'short';
      component.onSubmit();
      expect(component.error).toContain('au moins 8');
      expect(authService.register).not.toHaveBeenCalled();
    });

    it('should call AuthService.register with valid credentials', () => {
      authService.register.and.returnValue(of({}));
      authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'new@example.com' }));
      
      component.email = 'new@example.com';
      component.password = 'SecurePass123!';
      component.passwordConfirm = 'SecurePass123!';
      component.onSubmit();

      expect(authService.register).toHaveBeenCalledWith('new@example.com', 'SecurePass123!');
    });

    it('should display success message and then login on register success', fakeAsync(() => {
      authService.register.and.returnValue(of({}));
      authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'new@example.com' }));
      const navSpy = spyOn(router, 'navigate');

      component.email = 'new@example.com';
      component.password = 'SecurePass123!';
      component.passwordConfirm = 'SecurePass123!';
      component.onSubmit();

      tick(100);
      expect(component.success).toContain('Compte créé');
      
      tick(1500); // Wait for auto-login
      expect(authService.login).toHaveBeenCalled();
    }));

    it('should display error on registration failure', () => {
      authService.register.and.returnValue(throwError(() => ({ error: { message: 'Email already exists' } })));
      
      component.email = 'existing@example.com';
      component.password = 'SecurePass123!';
      component.passwordConfirm = 'SecurePass123!';
      component.onSubmit();

      expect(component.error).toContain('Email');
    });
  });

  describe('Toggle Mode', () => {
    it('should toggle between login and register mode', () => {
      expect(component.isRegistering).toBeFalsy();
      component.toggleMode();
      expect(component.isRegistering).toBeTruthy();
      component.toggleMode();
      expect(component.isRegistering).toBeFalsy();
    });

    it('should clear form when toggling mode', () => {
      component.email = 'test@example.com';
      component.password = 'password123';
      component.passwordConfirm = 'password123';
      component.error = 'Some error';
      component.success = 'Some success';

      component.toggleMode();

      expect(component.email).toBe('');
      expect(component.password).toBe('');
      expect(component.passwordConfirm).toBe('');
      expect(component.error).toBe('');
      expect(component.success).toBe('');
    });

    it('should show toggle button to switch mode', () => {
      const compiled = fixture.nativeElement as HTMLElement;
      const toggleButton = compiled.querySelector('button[type="button"]');
      expect(toggleButton).toBeTruthy();
      expect(toggleButton?.textContent).toContain('S\'inscrire');
    });
  });

  describe('HTML Rendering', () => {
    it('should render test credentials in login mode', () => {
      component.isRegistering = false;
      fixture.detectChanges();
      
      const compiled = fixture.nativeElement as HTMLElement;
      // Browser renders &#64; as @, so check the visible text
      expect(compiled.textContent).toContain('test'); // part of test@example.com
      expect(compiled.textContent).toContain('SecurePass123!');
    });
  });
});
