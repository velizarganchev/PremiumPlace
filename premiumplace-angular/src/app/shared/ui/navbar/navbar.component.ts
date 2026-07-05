import { Component, computed, inject } from '@angular/core';

import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../../core/auth/auth.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, MatToolbarModule, MatButtonModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss'
})
export class NavbarComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.auth.user;
  readonly isLoggedIn = this.auth.isLoggedIn;
  readonly isAdmin = this.auth.isAdmin;

  readonly username = computed(() => this.user()?.username ?? '');

  logout() {
    const shouldRedirectToLogin = this.router.url.startsWith('/admin');

    this.auth.logout().subscribe({
      next: () => {
        if (shouldRedirectToLogin) {
          this.router.navigateByUrl('/auth/login');
        }
      },
    });
  }
}
