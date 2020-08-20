import { HttpInterceptor, HttpHandler, HttpEvent, HttpRequest, HTTP_INTERCEPTORS } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { Observable, throwError } from "rxjs";
import { catchError } from "rxjs/operators";

import { AuthService } from "./auth.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
	constructor(private authService: AuthService, private router: Router) {}

	intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
		// send request with credential options in order to be able to read cross-origin cookies
		const req = request.clone({ withCredentials: true }); //QUESTO E' NECESSARIO PER PASSARE Cookie VIA CORS
		return next.handle(req).pipe(
			catchError(err => {
				if (err.status === 401 || err.status === 403) {
					// auto logout if 401 response returned from api
					//this.authService.logout();
					//window.location.reload(true);

					// redirect to login -> angular page that will call auth/login for CookieAuth
					let returnTo = this.router.url;
					alert("RETTO=\t" + returnTo);
					this.router.navigate(["/login"], { queryParams: { returnTo } });
					//this.router.navigateByUrl("/login", { queryParams: { returnTo } });
				}

				// propagate other errors
				return throwError(err);
			})
		);
	}
}

export const provideAuthInterceptor = {
	provide: HTTP_INTERCEPTORS,
	useClass: AuthInterceptor,
	multi: true
};
