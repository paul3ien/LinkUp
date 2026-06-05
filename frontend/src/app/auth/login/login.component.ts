// T070: Login Component – standalone, reactive form, redirects to /chat on success
import { Component, inject } from '@angular/core';

import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  passwordConfirm = '';
  error = '';
  success = '';
  loading = false;
  isRegistering = false;

  toggleMode(): void {
    this.isRegistering = !this.isRegistering;
    this.email = '';
    this.password = '';
    this.passwordConfirm = '';
    this.error = '';
    this.success = '';
  }

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.error = 'Email et mot de passe requis.';
      return;
    }

    if (this.isRegistering) {
      this.register();
    } else {
      this.login();
    }
  }

  private login(): void {
    this.loading = true;
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: () => {
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Email ou mot de passe invalide.';
        this.loading = false;
      }
    });
  }

  private register(): void {
    if (this.password !== this.passwordConfirm) {
      this.error = 'Les mots de passe ne correspondent pas.';
      return;
    }

    if (this.password.length < 8) {
      this.error = 'Le mot de passe doit avoir au moins 8 caractères.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.auth.register(this.email, this.password).subscribe({
      next: () => {
        this.success = 'Compte créé ! Connexion en cours...';
        setTimeout(() => this.router.navigate(['/profile']), 1000);
      },
      error: (err) => {
        this.error = err.error?.message || 'Erreur lors de la création du compte. Email peut être déjà utilisé.';
        this.loading = false;
      }
    });
  }
}
