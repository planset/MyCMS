import { Component, ViewChild, OnInit, ElementRef } from '@angular/core';

import { Post } from '../_models/index';
import { PostService } from '../_services/index';
import { PostEditComponent } from './post.component';

@Component({
    moduleId: module.id,
    templateUrl: 'admin.component.html'
})

export class AdminComponent implements OnInit {
    posts: Post[] = [];

    @ViewChild(PostEditComponent) postComponent: PostEditComponent;
    @ViewChild("fileInput") fileInput;
    @ViewChild("importButton") importButton: ElementRef;

    currentEditPost: Post = new Post();

    constructor(private postService: PostService) { }

    ngOnInit() {
        this.postService.getAllPosts()
            .subscribe(posts => {
                this.posts = posts;
            });
    }

    createNewPost(){
        this.currentEditPost = new Post();
    }

    editPost(post: Post) {
        this.currentEditPost = post;
    }

    import() {
        let fi = this.fileInput.nativeElement;
        if (fi.files && fi.files[0]) {
            let fileToUpload = fi.files[0];
            this.postService.import(fileToUpload)
                .subscribe(
                (d) => { 
                    console.log('success');
                    this.postService.getAllPosts()
                        .subscribe(posts => {
                            this.posts = posts;
                        });
                },
                (e) => { console.log('error'); },
                () => { this.importButton.nativeElement.removeAttribute('disabled'); }
            );
            this.importButton.nativeElement.setAttribute('disabled', 'disabled');
        }
    }

}