import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { NgModule, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatToolbarModule } from '@angular/material/toolbar';
import { BrowserModule } from '@angular/platform-browser';
import { provideEffects } from '@ngrx/effects';
import { provideStore } from '@ngrx/store';
import {
  AutoRefreshTokenService,
  INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  UserActivityService,
  includeBearerTokenInterceptor,
  provideKeycloak,
  withAutoRefreshToken
} from 'keycloak-angular';

import { App } from './app';
import { AppRoutingModule } from './app-routing-module';
import { errorInterceptor } from './core/error.interceptor';
import { AuthTokens } from './core/models';
import { loadSession, saveSession } from './core/session-storage';
import Keycloak from 'keycloak-js';

const authKey = 'auth.tokens';
const savedTokens = loadSession<AuthTokens>(authKey, {});

@NgModule({
  declarations: [
    App
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatSidenavModule,
    MatSnackBarModule,
    MatToolbarModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAppInitializer(() => {
      const keycloak = inject(Keycloak);
      const save = () =>
        saveSession(authKey, {
          token: keycloak.token,
          refreshToken: keycloak.refreshToken,
          idToken: keycloak.idToken
        });
      keycloak.onAuthSuccess = save;
      keycloak.onAuthRefreshSuccess = save;
      keycloak.onAuthLogout = () => sessionStorage.removeItem(authKey);
    }),
    provideKeycloak({
      config: {
        url: `${window.location.origin}/auth`,
        realm: 'mountypython',
        clientId: 'mountypython-client'
      },
      initOptions: {
        onLoad: 'login-required',
        pkceMethod: 'S256',
        checkLoginIframe: false,
        ...(savedTokens.token && savedTokens.refreshToken ? savedTokens : {})
      },
      features: [
        withAutoRefreshToken({ onInactivityTimeout: 'logout', sessionTimeout: 1800000 })
      ],
      providers: [AutoRefreshTokenService, UserActivityService]
    }),
    {
      provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
      useValue: [{ urlPattern: /^\/api\//i }]
    },
    provideHttpClient(withInterceptors([includeBearerTokenInterceptor, errorInterceptor])),
    provideStore(),
    provideEffects()
  ],
  bootstrap: [App]
})
export class AppModule { }
