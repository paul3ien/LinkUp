import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  private readonly auth = inject(AuthService);

  username = signal('');
  email = signal('');

  // Change username form
  newUsername = '';
  usernameMsg = '';
  usernameError = '';
  usernameLoading = false;

  // Change password form
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';
  passwordMsg = '';
  passwordError = '';
  passwordLoading = false;

  ngOnInit(): void {
    this.username.set(this.auth.getUsername() ?? '');
    this.email.set(this.auth.getEmail() ?? '');
  }

  saveUsername(): void {
    if (!this.newUsername.trim()) return;
    this.usernameLoading = true;
    this.usernameMsg = '';
    this.usernameError = '';
    this.auth.changeUsername(this.newUsername.trim()).subscribe({
      next: res => {
        this.username.set(res.username);
        this.newUsername = '';
        this.usernameMsg = 'Pseudo mis à jour !';
        this.usernameLoading = false;
      },
      error: err => {
        this.usernameError = err.error || 'Ce pseudo est déjà pris.';
        this.usernameLoading = false;
      }
    });
  }

  savePassword(): void {
    if (!this.currentPassword || !this.newPassword) return;
    if (this.newPassword !== this.confirmPassword) {
      this.passwordError = 'Les mots de passe ne correspondent pas.';
      return;
    }
    if (this.newPassword.length < 6) {
      this.passwordError = 'Le nouveau mot de passe doit avoir au moins 6 caractères.';
      return;
    }
    this.passwordLoading = true;
    this.passwordMsg = '';
    this.passwordError = '';
    this.auth.changePassword(this.currentPassword, this.newPassword).subscribe({
      next: () => {
        this.currentPassword = '';
        this.newPassword = '';
        this.confirmPassword = '';
        this.passwordMsg = 'Mot de passe mis à jour !';
        this.passwordLoading = false;
      },
      error: err => {
        this.passwordError = err.error || 'Mot de passe actuel incorrect.';
        this.passwordLoading = false;
      }
    });
  }

  logout(): void { this.auth.logout(); }
}
