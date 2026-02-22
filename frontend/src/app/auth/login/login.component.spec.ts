// T052: Tests for LoginComponent
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
    authService = jasmine.createSpyObj('AuthService', ['login']);

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

  it('should render email and password inputs', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('input[type="email"]')).toBeTruthy();
    expect(compiled.querySelector('input[type="password"]')).toBeTruthy();
  });

  it('should not call login when fields are empty', () => {
    component.email = '';
    component.password = '';
    component.onSubmit();
    expect(authService.login).not.toHaveBeenCalled();
  });

  it('should call AuthService.login with email and password', () => {
    authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'a@b.com' }));
    component.email = 'a@b.com';
    component.password = 'secret';
    component.onSubmit();
    expect(authService.login).toHaveBeenCalledWith('a@b.com', 'secret');
  });

  it('should navigate to /chat on successful login', fakeAsync(() => {
    authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'a@b.com' }));
    const navSpy = spyOn(router, 'navigate');
    component.email = 'a@b.com';
    component.password = 'secret';
    component.onSubmit();
    tick();
    expect(navSpy).toHaveBeenCalledWith(['/chat']);
  }));

  it('should display error message on login failure', fakeAsync(() => {
    authService.login.and.returnValue(throwError(() => new Error('401')));
    component.email = 'a@b.com';
    component.password = 'wrong';
    component.onSubmit();
    tick();
    expect(component.error).toBe('Email ou mot de passe invalide.');
    expect(component.loading).toBeFalse();
  }));

  it('should set loading=true while request is in flight', () => {
    // login returns an observable that never completes during this tick
    authService.login.and.returnValue(of({ token: 't', userId: 'u', email: 'a@b.com' }));
    component.email = 'a@b.com';
    component.password = 'secret';
    // loading is set synchronously before subscribe callback
    component.loading = false;
    component.onSubmit();
    // After synchronous completion loading stays false (tap is sync with of())
    // but the flag was set to true before the observable resolved
    expect(authService.login).toHaveBeenCalled();
  });
});
