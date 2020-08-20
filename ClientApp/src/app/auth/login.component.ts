import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { FormGroup, FormControl, Validators } from "@angular/forms";

import { AuthService } from "./auth.service";

@Component({
	selector: "app-login",
	template: `
		<form [formGroup]="frm" (ngSubmit)="login()">
			<label>Login:</label><input formControlName="Username" /><br />
			<label>Password:</label><input formControlName="Password" /><br />
			<button type="submit" [disabled]="frm.invalid">Login</button>
			<button type="button" app-logout>Logout</button>
		</form>
	`
})
export class LoginComponent {
	public returnTo: string;
	public frm = new FormGroup({
		Username: new FormControl("", Validators.required),
		Password: new FormControl("", Validators.required)
	});

	constructor(private authService: AuthService, private router: Router) {}
	ngOnInit(): void {
		this.router.events;
	}

	async login() {
		const currPage = this.router.parseUrl(this.router.url); //LEGGO returnTO dai queryParams
		const returnTo = currPage.queryParamMap.has("returnTo") ? currPage.queryParams.returnTo : "/";
		console.log({ returnTo });
		let ok = await this.authService.login(this.frm.value);
		if (ok) {
			alert("OK");
			this.router.navigateByUrl(returnTo || "/");
		} else {
			alert("LOGIN FAILED");
			this.frm.reset();
		}
	}
}
