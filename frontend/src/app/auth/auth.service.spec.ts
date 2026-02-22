// T052: Tests for AuthService
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { Component } from '@angular/core';
import { AuthService } from './auth.service';

@Component({ standalone: true, template: '' })
class DummyComponent {}

describe('AuthService', () => {
  let service: AuthService;
  let http: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [
        HttpClientTestingModule,
        RouterTestingModule.withRoutes([
          { path: 'login', component: DummyComponent },
          { path: '**', component: DummyComponent }
        ])
      ]
    });
    service = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    localStorage.clear();
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  // --- login ---
  it('should POST to /api/auth/login with credentials', () => {
    service.login('a@b.com', 'pass').subscribe();
    const req = http.expectOne('http://localhost:5001/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'a@b.com', password: 'pass' });
    req.flush({ token: 'tok', userId: 'uid1', email: 'a@b.com' });
  });

  it('should store token and userId in localStorage on successful login', fakeAsync(() => {
    service.login('a@b.com', 'pass').subscribe();
    http.expectOne('http://localhost:5001/api/auth/login')
        .flush({ token: 'my-jwt', userId: 'u42', email: 'a@b.com' });
    tick();
    expect(localStorage.getItem('lu_token')).toBe('my-jwt');
    expect(localStorage.getItem('lu_user')).toBe('u42');
  }));

  it('should return token from getToken()', () => {
    localStorage.setItem('lu_token', 'abc');
    expect(service.getToken()).toBe('abc');
  });

  it('should return null from getToken() when not set', () => {
    expect(service.getToken()).toBeNull();
  });

  it('should return userId from getUserId()', () => {
    localStorage.setItem('lu_user', 'uid999');
    expect(service.getUserId()).toBe('uid999');
  });

  // --- isLoggedIn ---
  it('should return true from isLoggedIn() when token exists', () => {
    localStorage.setItem('lu_token', 'tok');
    expect(service.isLoggedIn()).toBeTrue();
  });

  it('should return false from isLoggedIn() when no token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  // --- logout ---
  it('should clear localStorage and navigate to /login on logout', () => {
    localStorage.setItem('lu_token', 'tok');
    localStorage.setItem('lu_user', 'uid');
    const navSpy = spyOn(router, 'navigate');
    service.logout();
    expect(localStorage.getItem('lu_token')).toBeNull();
    expect(localStorage.getItem('lu_user')).toBeNull();
    expect(navSpy).toHaveBeenCalledWith(['/login']);
  });
});
