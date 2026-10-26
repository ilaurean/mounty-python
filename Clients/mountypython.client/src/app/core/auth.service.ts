import { Injectable, inject } from '@angular/core';
import Keycloak from 'keycloak-js';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly keycloak = inject(Keycloak);

  get username(): string {
    return (this.keycloak.tokenParsed?.['preferred_username'] as string | undefined) ?? '';
  }

  get roles(): string[] {
    return this.keycloak.realmAccess?.roles ?? [];
  }

  get isManager(): boolean {
    return this.roles.includes('manager');
  }

  get isCustomer(): boolean {
    return this.roles.includes('customer');
  }

  get token(): string | undefined {
    return this.keycloak.token;
  }

  async getFreshToken(): Promise<string> {
    await this.keycloak.updateToken(30).catch(() => this.keycloak.login());
    return this.keycloak.token ?? '';
  }

  logout(): Promise<void> {
    return this.keycloak.logout({ redirectUri: window.location.origin });
  }
}
