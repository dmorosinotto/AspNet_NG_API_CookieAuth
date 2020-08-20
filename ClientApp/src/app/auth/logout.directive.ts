import { Directive, HostListener } from "@angular/core";
import { Router } from "@angular/router";
import { AuthService } from "./auth.service";

@Directive({
	selector: "[app-logout]"
})
export class LogoutDirective {
	constructor(private authService: AuthService, private router: Router) {}

	@HostListener("click", ["$event"]) logout($event: MouseEvent) {
		$event.preventDefault();
		this.authService.logout();
		alert("LOGGED OUT");
		this.router.navigateByUrl("/");
	}
}
