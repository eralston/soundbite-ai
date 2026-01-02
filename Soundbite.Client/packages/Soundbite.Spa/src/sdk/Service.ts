import Axios, { AxiosInstance, AxiosRequestConfig } from "axios";
import SpaStore from "../store/Spa.Store";

/**
 * Class that marries an Axios instance to the app's server-side API
 * This uses the current Auth settings defined in the Auth class
 * */
export default class Service {
  /**
   * Expose Axios for custom calls even outside this object
   */
  private _axios?: AxiosInstance = undefined;

  public get axios(): AxiosInstance {
    if (this._axios === undefined) {
      console.log("API URI:", SpaStore.config?.adClientConfig.apiUri);

      this._axios = Axios.create({
        baseURL: SpaStore.config?.adClientConfig.apiUri, // TODO: Consider CORS + independent API domain
        timeout: undefined, // TODO: Consider making this configurable
      });

      this.addAuthInterceptor(this._axios);
    }

    return this._axios;
  }

  async addAuthInterceptor(instance: AxiosInstance) {
    // Setup interceptor for Auth Strategy to be injected into headers
    instance.interceptors.request.use(
      async (config: AxiosRequestConfig) => {
        const accessToken = SpaStore.token;
        config.headers.Authorization = `Bearer ${accessToken}`;
        return config;
      },
      (error: Error) => {
        return Promise.reject(error);
      }
    );
  }

  async getAsync<Type>(path: string): Promise<Type> {
    const response = await this.axios.get<Type>(path);
    return response.data;
  }

  async getBlobAsync(path: string): Promise<Blob> {
    const response = await this.axios.get(path, {
      responseType: "arraybuffer",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/pdf",
      },
    });

    // Check for errors in Axios response
    if (
      response.status !== 200 ||
      response.headers["content-type"]?.startsWith("application/json")
    ) {
      return new Blob();
    }

    // If we're good, then return the blob
    return new Blob([response.data]);
  }

  async postAsync<Type>(path: string, data: any): Promise<Type> {
    const response = await this.axios.post<Type>(path, data);
    return response.data;
  }
}
