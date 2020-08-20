import { NgModule, ModuleWithProviders } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { HttpClientModule } from "@angular/common/http";
import { ReactiveFormsModule } from "@angular/forms";

import { provideErrorInterceptor } from "./error.interceptor";
import { LoginComponent } from "./login.component";
import { LogoutDirective } from "./logout.directive";
//import { AuthService } from "./auth.service";
//import { AuthGuard } from "./auth.guard";
//export { AuthGuard, AuthService };

@NgModule({
	declarations: [LoginComponent, LogoutDirective],
	imports: [
		CommonModule,
		ReactiveFormsModule,
		HttpClientModule,
		RouterModule.forChild([{ path: "login", component: LoginComponent }])
	],
	exports: [LoginComponent, LogoutDirective]
})
export class AuthModule {
	static forRoot(): ModuleWithProviders<AuthModule> {
		return {
			ngModule: AuthModule,
			providers: [provideErrorInterceptor /*, AuthService, AuthGuard*/]
		};
	}
}
