import { Component, Input, OnInit } from '@angular/core';

import { Post } from '../_models/index';
import { PostService } from '../_services/index';

@Component({
    selector: "post-edit",
    moduleId: module.id,
    templateUrl: './post.component.html',
    styleUrls: ["./post.component.scss"]
})
export class PostEditComponent implements OnInit {

    @Input() post: Post;

    constructor(private postService: PostService) { }

    ngOnInit() {
    }

    savePost() {
        this.postService.postPost(this.post)
            .subscribe();
    }

}