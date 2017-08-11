import { environment } from '../environments/environment';

export class AppSettings {
    public static AUTH_ENDPOINT = environment.authEndpoint;
    public static API_ENDPOINT = environment.apiEndpoint;
}
