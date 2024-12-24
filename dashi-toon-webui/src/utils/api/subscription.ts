import { fetchApi } from ".";
import { Result } from "../err";

export async function cancelSubscription(subId: string): Promise<Result<void>> {
    return await fetchApi("PUT", `/api/Users/subscriptions/${subId}/cancel`);
}

export interface Series {
    id: number;
    thumbnailUrl: string;
    title: string;
    author: string;
    lastModified: string;
}

export interface DashiFan {
    id: string;
    name: string;
    description: string;
    perks: number;
    price: { amount: number; currency: string };
}

export interface Subscription {
    subscriptionId: string;
    series: Series;
    dashiFan: DashiFan;
    nextBillingDate: string;
    subscribedSince: string;
    status: number;
}

export async function getSubscriptions(): Promise<Result<Subscription[]>> {
    const [data, err] = await fetchApi<Subscription[]>("GET", `/api/Users/subscriptions`);

    if (err) {
        return [data, err];
    }

    // Backend dont update so we update ourself :v
    const tf = data
        .filter((x) => x.status === 1 || x.status === 2 || x.status === 4 || x.status === 5)
        .map((x) => ({ ...x, status: x.status === 5 ? 4 : x.status }));
    return [tf, undefined];
}

export interface Subscription2 {
    subscriptionId: string;
    tier: DashiFan;
    nextBillingDate: string;
    status: number;
}

export async function getActiveSub(
    seriesId: string,
    preventAutoLogin?: boolean,
): Promise<Result<Subscription2 | null>> {
    return await fetchApi("GET", `/api/Users/subscriptions/series/${seriesId}`, undefined, {
        preventAutoLogin,
    });
}

export async function downgrade(tierId: string, subscriptionId: string): Promise<Result<Subscription2 | null>> {
    return await fetchApi("PUT", `/api/Users/subscriptions/${subscriptionId}/downgrade`, {
        subscriptionId,
        tierId,
    });
}
