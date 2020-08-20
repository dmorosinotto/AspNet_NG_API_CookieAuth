import { Injectable, Inject } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { map, finalize } from "rxjs/operators";

interface ILoginModel {
	Username: string;
	Password: string;
}

interface IUserProfile {}

@Injectable({ providedIn: "root" })
export class AuthService {
	private readonly _authUrl!: string;
	private _userProfile = new BehaviorSubject<IUserProfile>(null);

	constructor(private http: HttpClient, @Inject("BASE_URL") baseUrl: string, private router: Router) {
		this._authUrl = baseUrl + "auth/login";
	}

	public login(credential: ILoginModel): Promise<boolean> {
		const currPage = this.router.parseUrl(this.router.url);
		const returnTo = currPage.queryParamMap.has("returnTo") ? currPage.queryParams.returnTo : "/";
		const loginUrl = this._authUrl;
		return this.http
			.post<IUserProfile>(loginUrl, credential)
			.toPromise()
			.then(data => {
				this._userProfile.next(data);
				return Boolean(data);
			})
			.catch(err => {
				this._userProfile = null;
				console.error("LOGIN ERROR", err);
				return false;
			});
	}

	public logout(): void {
		this.http
			.get(this._authUrl.replace("login", "logout"))
			.pipe(finalize(() => this._userProfile.next(null)))
			.subscribe();
	}

	public get isLoggedIn(): boolean {
		return this._userProfile.getValue() != null;
	}
	public userProfile$ = this._userProfile.asObservable();
	public isLoggedIn$ = this._userProfile.pipe(map(u => !!u));

	private _hasRole = (user: IUserProfile, role?: string) => !!role || true; //FAKE PER ORA NON GESTICO RUOLI
	public inRole = (role?: string) => this._hasRole(this._userProfile.getValue, role);
	public isInRole$ = (role?: string) => this._userProfile.pipe(map(u => this._hasRole(u, role)));
}
