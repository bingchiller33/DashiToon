import * as env from "@/utils/env";
import { promiseErr, Result } from "@/utils/err";
import { toast } from "sonner";
import { delay } from "..";

export interface FetchOptions {
    params?: Record<string, any>;
    preventAutoLogin?: boolean;
    extras?: RequestInit;
}

export async function fetchApi<R = any>(
    method: "GET" | "POST" | "PUT" | "DELETE",
    route: string,
    body?: any,
    opt?: FetchOptions,
): Promise<Result<R>> {
    const url = new URL(env.getBackendHost()!);
    url.pathname = route;
    for (const [k, v] of Object.entries(opt?.params ?? {})) {
        if (typeof v === "undefined") {
            continue;
        }
        if (Array.isArray(v)) {
            v.forEach((x) => url.searchParams.append(k, x.toString()));
        } else {
            url.searchParams.append(k, v.toString());
        }
    }

    let bd: any;
    let headers: any = {};
    if (body instanceof FormData) {
        bd = body;
    } else {
        bd = JSON.stringify(body);
        headers = {
            "Content-Type": "application/json",
        };
    }

    const [resp, err] = await promiseErr(
        fetch(url, {
            credentials: "include",
            body: bd,
            method,
            headers: headers || undefined,
            ...opt?.extras,
        }),
    );

    if (err) {
        console.error("Failed to fetch!", err);
        return [undefined, err];
    }

    if (resp.status === 401 && !opt?.preventAutoLogin) {
        window.location.href = `/login?returnUrl=${window.location.pathname}${window.location.search}`;
        return [undefined, "Chuyển hướng đến đăng nhập"];
    }

    if (resp.status === 403) {
        toast?.error("Bạn không có quyền xem nội dung này!");
        return [undefined, "Không cho phép!"];
    }

    const [text, terr] = await promiseErr(resp.text());
    if (terr) {
        return [undefined, terr];
    }

    if (resp.status >= 400) {
        const text = await resp.text().catch((e) => console.error(e));
        console.error("Server return a erroneous status");
        return [undefined, { text, status: resp.status }];
    }

    const isJson = resp.headers.get("Content-Type")?.startsWith("application/json");
    return [isJson ? JSON.parse(text) : text, undefined] as Result<R>;
}

export async function fakeApi<T>(val: T, delayMs?: number): Promise<Result<T>> {
    await delay(delayMs ?? 1000);
    return [val, undefined];
}

export async function tryLocalnet() {
    const resp = await fetch("/localnet", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: window.location.href,
    });

    const data = await resp.json();
    if (data.base) {
        localStorage.setItem("rewriteBase", data.base);
        toast.success("Detected local network access, writing base!");
    } else {
        localStorage.removeItem("rewriteBase");
    }
}

export interface Paginated<T> {
    items: T[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
}
