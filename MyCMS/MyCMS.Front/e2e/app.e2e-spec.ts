import { App2scssPage } from './app.po';

describe('app2scss App', function() {
  let page: App2scssPage;

  beforeEach(() => {
    page = new App2scssPage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
