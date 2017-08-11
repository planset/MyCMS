import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule }    from '@angular/forms';
import { HttpModule } from '@angular/http';
import { BaseRequestOptions, Http } from '@angular/http';

import { AppComponent }  from './app.component';
import { routing }        from './app.routing';

import { AuthGuard } from './_guards/index';
import { AuthenticationService, PostService } from './_services/index';
import { LoginComponent } from './login/index';
import { HomeComponent } from './home/index';
import { PostDetailComponent } from './post/index';
import { AdminComponent, PostEditComponent } from './admin/index';
import { AuthenticatedHttpService } from "./_services/AuthenticatedHttpService";

@NgModule({
    imports: [
        BrowserModule,
        FormsModule,
        HttpModule,
        routing
    ],
    declarations: [
        AppComponent,
        LoginComponent,
        AdminComponent,
        PostDetailComponent,
        PostEditComponent,
        HomeComponent
    ],
    providers: [
        { provide: Http, useClass: AuthenticatedHttpService },

        AuthGuard,
        AuthenticationService,
        PostService,

        BaseRequestOptions
    ],
    bootstrap: [AppComponent]
})

export class AppModule { }