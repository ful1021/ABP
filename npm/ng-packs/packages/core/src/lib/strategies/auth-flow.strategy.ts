import { Injector } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngxs/store';
import { AuthConfig, OAuthService } from 'angular-oauth2-oidc';
import { Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { GetAppConfiguration } from '../actions/config.actions';
import { RestOccurError } from '../actions/rest.actions';
import { RestService } from '../services/rest.service';
import { ConfigState } from '../states/config.state';

export abstract class AuthFlowStrategy {
  abstract readonly isInternalAuth: boolean;

  protected store: Store;
  protected oAuthService: OAuthService;
  protected oAuthConfig: AuthConfig;
  abstract checkIfInternalAuth(): boolean;
  abstract login(): void;
  abstract logout(): Observable<any>;
  abstract destroy(): void;

  private catchError = err => this.store.dispatch(new RestOccurError(err));

  constructor(protected injector: Injector) {
    this.store = injector.get(Store);
    this.oAuthService = injector.get(OAuthService);
    this.oAuthConfig = this.store.selectSnapshot(ConfigState.getDeep('environment.oAuthConfig'));
  }

  async init(): Promise<any> {
    this.oAuthService.configure(this.oAuthConfig);
    return this.oAuthService.loadDiscoveryDocument().catch(this.catchError);
  }
}

export class AuthCodeFlowStrategy extends AuthFlowStrategy {
  readonly isInternalAuth = false;

  async init() {
    return super
      .init()
      .then(() => this.oAuthService.tryLogin())
      .then(() => {
        if (this.oAuthService.hasValidAccessToken() || !this.oAuthService.getRefreshToken()) {
          return Promise.resolve();
        }

        return this.oAuthService.refreshToken() as Promise<any>;
      })
      .then(() => this.oAuthService.setupAutomaticSilentRefresh({}, 'access_token'));
  }

  login() {
    this.oAuthService.initCodeFlow();
  }

  checkIfInternalAuth() {
    this.oAuthService.initCodeFlow();
    return false;
  }

  logout() {
    this.oAuthService.logOut();
    return of(null);
  }

  destroy() {}
}

export class AuthPasswordFlowStrategy extends AuthFlowStrategy {
  readonly isInternalAuth = true;

  login() {
    const router = this.injector.get(Router);
    router.navigateByUrl('/account/login');
  }

  checkIfInternalAuth() {
    return true;
  }

  logout() {
    const rest = this.injector.get(RestService);

    const issuer = this.store.selectSnapshot(ConfigState.getDeep('environment.oAuthConfig.issuer'));
    return rest
      .request(
        {
          method: 'GET',
          url: '/api/account/logout',
        },
        null,
        issuer,
      )
      .pipe(
        tap(() => this.oAuthService.logOut()),
        switchMap(() => this.store.dispatch(new GetAppConfiguration())),
      );
  }

  destroy() {}
}

export const AUTH_FLOW_STRATEGY = {
  Code(injector: Injector) {
    return new AuthCodeFlowStrategy(injector);
  },
  Password(injector: Injector) {
    return new AuthPasswordFlowStrategy(injector);
  },
};
