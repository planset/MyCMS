export class Post {
    public partitionKey: string;
    public rowKey: string;

    public title: string;
    public content: string;
    public publishDate: Date;
    public status: string;

    public categories: string;
    public tags: string;

    public createData: Date;
    public modifyData: Date;

    public wpPostId: number;
    public wpLink: string;
}