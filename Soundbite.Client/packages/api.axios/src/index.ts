import { SoundbiteApiConfig } from "@soundbite/api";
import Axios from "axios";
import { AxiosHttpAdapter } from "./AxiosHttpAdapter";

/**
 * Configures the Soundbite API to use an Axios client for HTTP calls.
 **/
export function setupAxiosAdapter(): void {
  SoundbiteApiConfig.httpAdapter = new AxiosHttpAdapter();
}

/**
 * Download the given link as a blob
 * @param medialUrl
 */
export async function downloadBlob(medialUrl: string): Promise<Blob> {
  const response = await Axios.get(medialUrl, {
    method: "GET",
    responseType: "blob",
  });
  return response.data;
}
