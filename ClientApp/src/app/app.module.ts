import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { RouterModule } from "@angular/router";

import { AppComponent } from "./app.component";
import { NavMenuComponent } from "./nav-menu/nav-menu.component";
import { HomeComponent } from "./home/home.component";
import { CounterComponent } from "./counter/counter.component";
import { FetchDataComponent } from "./fetch-data/fetch-data.component";
import { AuthModule, LoginComponent, AuthGuard } from "./auth/public-api";

@NgModule({
	declarations: [AppComponent, NavMenuComponent, HomeComponent, CounterComponent, FetchDataComponent],
	imports: [
		BrowserModule.withServerTransition({ appId: "ng-cli-universal" }),
		AuthModule.forRoot(),
		RouterModule.forRoot(
			[
				{ path: "", component: HomeComponent, pathMatch: "full" },

				{ path: "counter", component: CounterComponent, canActivate: [AuthGuard] },
				{ path: "fetch-data", component: FetchDataComponent }
			],
			{ useHash: true }
		)
	],
	bootstrap: [AppComponent]
})
export class AppModule {}
