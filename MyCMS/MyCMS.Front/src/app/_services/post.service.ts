import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { Observable } from 'rxjs';
import 'rxjs/add/operator/map'

import { AuthenticationService } from './index';
import { Post } from '../_models/index';
import { AppSettings } from '../appSettings';

@Injectable()
export class PostService {
    constructor(
        private http: Http,
        private authenticationService: AuthenticationService) {
    }

    getPost(id: string): Observable<Post> {
        let headers = new Headers();
        let options = new RequestOptions({ headers: headers });

        return this.http.get(AppSettings.API_ENDPOINT + '/posts/' + id, options)
            .map((response: Response) => response.json());
    }

    getPosts(): Observable<Post[]> {
        let headers = new Headers();
        let options = new RequestOptions({ headers: headers });

        return this.http.get(AppSettings.API_ENDPOINT + '/posts', options)
            .map((response: Response) => response.json());
    }

    getAllPosts(): Observable<Post[]> {
        let headers = new Headers({ 
            'Authorization': 'Bearer ' + this.authenticationService.token
        });
        let options = new RequestOptions({ headers: headers });

        return this.http.get(AppSettings.API_ENDPOINT + '/posts/allposts', options)
            .map((response: Response) => response.json());
    }

    postPost(post: Post): Observable<any> {
        let headers = new Headers({ 
            'Authorization': 'Bearer ' + this.authenticationService.token,
            'Content-Type': 'application/json'
        });
        let options = new RequestOptions({ headers: headers });

        return this.http.post(AppSettings.API_ENDPOINT + '/posts',
            JSON.stringify(post),
            options
        )
            .map((res:Response) => res.json())
            .catch((error: any) => Observable.throw(error.json().error || 'Server error'));
    }

    import(importFile: File){
        let headers = new Headers({ 
            'Authorization': 'Bearer ' + this.authenticationService.token,
            'Accept': 'application/json'
        });
        let options = new RequestOptions({ headers: headers });
        let formData: FormData = new FormData();
        formData.append('importFile', importFile, importFile.name);

        return this.http.post(AppSettings.API_ENDPOINT + '/posts/import',
            formData,
            options
        )
            .map((res:Response) => res.json())
            .catch((error: any) => Observable.throw(error.json().error || 'Server error'));
    }
}