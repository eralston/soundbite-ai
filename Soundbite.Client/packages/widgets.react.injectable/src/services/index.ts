import { setupAxiosAdapter } from "@soundbite/api-axios"

// Use the axios adapter for all injectable widgets
setupAxiosAdapter();

export * from './WidgetInjector';
export { SoundbiteApiConfig } from "@soundbite/api";
