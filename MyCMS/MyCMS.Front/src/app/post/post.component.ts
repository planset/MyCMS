import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Post } from '../_models/index';
import { PostService } from '../_services/index';

@Component({
    selector: "post-detail",
    moduleId: module.id,
    templateUrl: './post.component.html',
    styleUrls: ["./post.component.scss"]
})
export class PostDetailComponent implements OnInit, OnDestroy {

    post: Post;

    id: string;
    sub: any;

    constructor(
        private route: ActivatedRoute,
        private postService: PostService) { }

    ngOnInit() {
        this.post = new Post();
        this.sub = this.route.params.subscribe(params => {
            this.id = params['id'];

            this.postService.getPost(this.id)
                .subscribe((post) => { 
                    this.post = post 
                });
        });
    }

    ngOnDestroy() {
        this.sub.unsubscribe();
    }
}