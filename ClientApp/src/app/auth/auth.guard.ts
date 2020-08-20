import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import {
	CanActivate,
	CanLoad,
	ActivatedRouteSnapshot,
	RouterStateSnapshot,
	Router,
	Route,
	UrlSegment
} from "@angular/router";

import { AuthService } from "./auth.service";

@Injectable({
	providedIn: "root"
})
export class AuthGuard implements CanActivate, CanLoad {
	// add the service we need
	constructor(private authService: AuthService, private router: Router) {}
	canLoad(route: Route, segments: UrlSegment[]): boolean | Promise<boolean> | Observable<boolean> {
		const url = segments.reduce((path, currentSegment) => {
			return `${path}/${currentSegment.path}`;
		}, "");
		const role = route.data && route.data.role;
		return this._verify({ url, role });
	}

	canActivate(
		route: ActivatedRouteSnapshot,
		state: RouterStateSnapshot
	): Observable<boolean> | Promise<boolean> | boolean {
		const url = state.url;
		const role = route.data && route.data.role;
		return this._verify({ url, role });
	}

	private _verify({ url, role }: { url: string; role?: string }): boolean {
		if (this.authService.isLoggedIn && this.authService.inRole(role)) {
			return true;
		} else {
			// redirect the user to login (passing current page for returnTo)
			this.router.navigate(["/login"], { queryParams: { returnTo: url } });
			return false;
		}
	}
}
