import { Routes, RouterModule } from '@angular/router';

import { LoginComponent } from './login/index';
import { AdminComponent } from './admin/index';
import { HomeComponent } from './home/index';
import { PostDetailComponent } from './post/index';
import { AuthGuard } from './_guards/index';

const appRoutes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'admin', component: AdminComponent, canActivate: [AuthGuard] },
    { path: 'post/:id', component: PostDetailComponent },
    { path: '', component: HomeComponent },

    // otherwise redirect to home
    { path: '**', redirectTo: '' }
];

//export const routing = RouterModule.forRoot(appRoutes, {useHash:true});
export const routing = RouterModule.forRoot(appRoutes);