export const SIGNALR_URL = process.env["NEXT_PUBLIC_SIGNALR_URL"];
export const BACKEND_HOST = process.env["NEXT_PUBLIC_BACKEND_HOST"];
export const PAYPAL_CLIENT_ID = process.env["NEXT_PUBLIC_PAYPAL_CLIENT_ID"]!;
export const DEVTOOL_BYPASS = process.env["NEXT_PUBLIC_DEVTOOL_BYPASS"];
export const GA_ID = process.env["NEXT_PUBLIC_GA_ID"];
export const GA_PROP_ID = process.env["NEXT_PUBLIC_GA_PROP_ID"];

export function getBackendHost() {
    const localStorage = globalThis.localStorage;
    if (!localStorage) return BACKEND_HOST;

    const base = localStorage.getItem("rewriteBase");
    const enable = localStorage.getItem("rr");
    if (enable !== "true") return BACKEND_HOST;
    return base || BACKEND_HOST;
}
