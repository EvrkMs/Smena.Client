// В WebView2 ЛЮБОЙ вызов через chrome.webview.hostObjects.api.* — Promise,
// даже если соответствующий C#-метод объявлен как синхронный. Поэтому здесь
// вообще нет невернувших-Promise членов — если метод существует, он асинхронный.
export interface NativeApiHostObject {
  GetConstantsAsync(): Promise<string>;
  GetEmployeesAsync(): Promise<string>;
  AddEmployeeAsync(employeeJson: string): Promise<string>;
  GetCurrentSalaryAsync(employeeId: string): Promise<string>;
  GetCurrentSafeAsync(): Promise<string>;
  AddSafeComingAsync(operationJson: string): Promise<string>;
  SendAdvanceAsync(requestJson: string): Promise<string>;
  SendExpenseAsync(requestJson: string): Promise<string>;
  SendExpenseWithPhotoAsync(requestJson: string): Promise<string>;
  SendRaportWithPhotoAsync(requestJson: string): Promise<string>;
  SearchWarehouseItemsAsync(query: string, limit: number): Promise<string>;
  GetAllWarehouseItemsAsync(): Promise<string>;
}

export interface WebViewMessageEvent extends Event {
  data: unknown;
}

export interface ChromeWebView {
  hostObjects: {
    api: NativeApiHostObject;
  };
  addEventListener(type: 'message', listener: (ev: WebViewMessageEvent) => void): void;
  removeEventListener(type: 'message', listener: (ev: WebViewMessageEvent) => void): void;
  postMessage(message: unknown): void;
}

declare global {
  interface Window {
    chrome?: {
      webview?: ChromeWebView;
    };
  }
}
