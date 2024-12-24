export function isServer() {
    return typeof window === "undefined";
}

export interface NextPageProps {
    params: { [k: string]: string };
    searchParams: { [k: string]: string };
}
