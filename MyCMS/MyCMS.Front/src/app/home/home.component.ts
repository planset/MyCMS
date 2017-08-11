import { Component, OnInit } from '@angular/core';

import { Post } from '../_models/index';
import { PostService } from '../_services/index';

@Component({
    moduleId: module.id,
    templateUrl: 'home.component.html'
})

export class HomeComponent implements OnInit {
    posts: Post[] = [];

    constructor(private postService: PostService) { }

    ngOnInit() {
        // get users from secure api end point
        this.postService.getPosts()
            .subscribe(posts => {
                this.posts = posts;
            });
    }

}