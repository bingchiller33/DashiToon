import { fakeApi, fetchApi, Paginated } from ".";
import { Result } from "../err";
import * as env from "@/utils/env";
import { getActiveSub } from "./subscription";
import * as gtag from "@/utils/gtag";

export interface UpdatedSeries extends SeriesResp2 {
    chapterId: number;
    chapterNumber: number;
    chapterName: string;
    updatedAt: string;
}

export async function getRecentlyUpdatedSeries(): Promise<Result<UpdatedSeries[]>> {
    // const [data, err] = await search({ term: "a" });
    // if (err) {
    //     return [undefined, err];
    // }

    // return [
    //     data.items.map((x) => ({
    //         ...x,
    //         chapterId: 1,
    //         chapterNumber: 69,
    //         chapterName: "Cook",
    //         updatedAt: new Date().toDateString(),
    //     })),
    //     undefined,
    // ];
    return fetchApi("GET", "/api/Homepage/recently-updated-series", undefined);
}

export async function getRecommendedSeries(
    pageNumber: number,
    pageSize?: number,
): Promise<Result<Paginated<SeriesResp2>>> {
    // const [data, err] = await search({ term: "a", pageSize: pageSize });
    // if (err) {
    //     return [undefined, err];
    // }

    // return [
    //     {
    //         items: data.items,
    //         pageNumber: 1,
    //         totalCount: 100,
    //         totalPages: 10,
    //     },
    //     undefined,
    // ];

    return fetchApi("GET", "/api/Homepage/recommended-series", undefined, {
        params: {
            pageNumber,
            pageSize,
        },
    });
}

export async function getRecentlyReleasedSeries(): Promise<Result<SeriesResp2[]>> {
    // const [data, err] = await search({ term: "a" });
    // if (err) {
    //     return [undefined, err];
    // }

    // return [data.items, undefined];

    return fetchApi("GET", "/api/Homepage/recently-released-series", undefined);
}

export async function getPopularSeries(genreId: string): Promise<Result<SeriesResp2[]>> {
    // const [data, err] = await search({ term: "a" });
    // if (err) {
    //     return [undefined, err];
    // }

    // return [data.items, undefined];

    return fetchApi("GET", "/api/Homepage/trending-genres-series", undefined, {
        params: {
            genreId,
        },
    });
}

export function getTrendingSeries(periodId: string): Promise<Result<SeriesResp2[]>> {
    //
    // const [data, err] = await search({ term: "a" });
    // if (err) {
    //     return [undefined, err];
    // }

    // return [data.items, undefined];

    return fetchApi("GET", "/api/Homepage/trending-series", undefined, {
        params: {
            interval: periodId,
        },
    });
}

export interface GenreResp {
    id: number;
    name: string;
}

export function getGenres(): Promise<Result<GenreResp[]>> {
    return fetchApi("GET", "/api/Genres");
}

export function getTrendingGenres(): Promise<Result<GenreResp[]>> {
    return fetchApi("GET", "/api/Homepage/trending-genres");
}

export interface SearchResponse {
    items: SeriesResp2[];
    totalCount: number;
    suggestions: string[];
}

export interface SearchOptions {
    term: string;
    type?: number[];
    status?: string[];
    contentRating?: string[];
    genres?: string[];
    pageNumber?: number;
    pageSize?: number;
}

export async function search(opts: SearchOptions): Promise<Result<SearchResponse>> {
    try {
        gtag.eventSearch(opts);
    } catch {}
    return await fetchApi("GET", `/api/search`, undefined, {
        params: {
            ...opts,
        },
    });
}

export interface Subscription {
    subscriptionId: string;
    tier: DashiFanTier;
    status: number;
    nextBillingDate: string;
}

export interface SubscribeDashiFanTier {
    seriesId: number;
    tierId: string;
    returnUrl: string;
    cancelUrl: string;
}

export async function subscribeDashiFanTier(data: SubscribeDashiFanTier): Promise<Result<string>> {
    return await fetchApi("POST", `/api/Series/${data.seriesId}/dashi-fans/${data.tierId}/subscribe`, data);
}

export interface DashiFanTier {
    id: string;
    name: string;
    price: {
        amount: number;
        currency: string;
    };
    perks: number;
}

export async function getDashiFanTiers(id: string): Promise<Result<DashiFanTier[]>> {
    return await fetchApi("GET", `/api/Series/${id}/dashi-fans`);
}

export interface Chapter {
    id: number;
    chapterNumber: number;
    title: string;
    thumbnail: string;
    publishedDate: string;
    price: number;
}

export async function getChapters(id: string, volumeId: string): Promise<Result<Chapter[]>> {
    return await fetchApi("GET", `/api/Series/${id}/volumes/${volumeId}/chapters`);
}

export type VolumeWithChapters = Volume & { chapters: Chapter[] };
export interface VolumeChapters {
    latestChapter: Chapter | undefined;
    latestChapterVol: VolumeWithChapters | undefined;
    firstChap: Chapter | undefined;
    firstChapVol: VolumeWithChapters | undefined;
    data: VolumeWithChapters[];
}

export async function getUserAccessibleChapters(id: string): Promise<Result<VolumeChapters>> {
    const [volumes, errVol] = await getVolumes(id);
    if (errVol) {
        return [undefined, errVol];
    }

    const fetchChapters = await Promise.all(
        volumes.map(async (x) => ({
            ...x,
            chapters: await getChapters(id, x.volumeId.toString()),
        })),
    );

    const err = fetchChapters.find((x) => x.chapters[1] !== undefined);
    if (err) {
        return [undefined, err.chapters];
    }

    const volChaps = fetchChapters.map((x) => ({
        ...x,
        chapters: x.chapters[0] as Chapter[],
    }));

    const [activeSub, errA] = await getActiveSub(id, true);
    let perks = activeSub?.tier.perks ?? 0;

    const allChapters = volChaps
        .flatMap((x) => x.chapters)
        .filter((x) => !!x.publishedDate)
        .toSorted((a, b) => +new Date(a.publishedDate) - +new Date(b.publishedDate));

    const lastPublished = allChapters.findLastIndex((x) => new Date() > new Date(x.publishedDate));

    allChapters.splice(lastPublished + perks + 1);

    const filtered = volChaps.map((x) => ({
        ...x,
        chapters: x.chapters.filter((x) => allChapters.some((a) => x.id === a.id)),
    }));

    const latestChapterVol = filtered.findLast((x) => true);
    const latestChapter = latestChapterVol?.chapters.findLast((x) => true);

    const firstChapVol = filtered
        .toSorted((a, b) => a.volumeNumber - b.volumeNumber)
        .find((x) => x.chapters.length > 0);
    const firstChap = firstChapVol?.chapters.toSorted((a, b) => a.chapterNumber - b.chapterNumber).find((x) => true);

    const ret = {
        latestChapter,
        latestChapterVol,
        firstChap,
        firstChapVol,
        data: filtered,
    };
    return [ret, undefined];
}

export interface Volume {
    volumeId: number;
    volumeNumber: number;
    name: string;
    introduction: string;
}

export async function getVolumes(id: string): Promise<Result<Volume[]>> {
    return await fetchApi("GET", `/api/Series/${id}/volumes`);
}

export interface Genre {
    id: number;
    title: string;
}

export interface Analytics {
    rating: number;
    reviewCount: number;
    followCount: number;
    viewCount: number;
    lastModified: string;
}

export async function getSeriesAnalytics(id: string): Promise<Result<Analytics>> {
    return await fetchApi("GET", `/api/Series/${id}/analytics`);
}

export interface SeriesResp {
    id: number;
    title: string;
    alternativeTitles: string[];
    author: string;
    status: number;
    synopsis: string;
    thumbnail: string;
    type: number;
    genres: Genre[];
    contentRating: number;
}

export interface SeriesResp2 {
    id: number;
    title: string;
    authors: string[];
    status: number;
    synopsis: string;
    rating: number;
    thumbnail: string;
    type: number;
    genres: string[];
    contentRating: number;
}

export interface RelatedSeries {}

export async function getRelatedSeries(id: string): Promise<Result<RelatedSeries[]>> {
    return await fetchApi("GET", `/api/Series/${id}/Related`);
}

export async function getSeriesInfo(id: string): Promise<Result<SeriesResp>> {
    return await fetchApi("GET", `/api/Series/${id}`);
}

export interface ChapterContent<T> {
    id: number;
    title: string;
    thumbnail: string;
    content: T;
    chapterNumber: number;
}

export async function getNovelChapterContent(
    id: string,
    volumeId: string,
    chapterId: string,
): Promise<
    Result<
        ChapterContent<string>,
        {
            error: any;
            status: number;
        }
    >
> {
    try {
        const response = await fetch(
            `${env.BACKEND_HOST}/api/ReadContent/series/${id}/volumes/${volumeId}/chapters/${chapterId}/novel`,
            {
                credentials: "include",
                method: "GET",
            },
        );

        const status = response.status;
        if (!response.ok) {
            const text = await response.text();
            return [undefined, { error: text, status }];
        }

        const data = await response.json();
        return [data, undefined];
    } catch (error) {
        return [undefined, { error, status: 500 }];
    }
}

export interface ComicImage {
    imageUrl: string;
    imageName: string;
    imageSize: number;
    imageWidth: number;
    imageHeight: number;
}

export async function getComicChapterContent(
    id: string,
    volumeId: string,
    chapterId: string,
): Promise<Result<ChapterContent<ComicImage[]>, { error: any; status: number }>> {
    try {
        const response = await fetch(
            `${env.BACKEND_HOST}/api/ReadContent/series/${id}/volumes/${volumeId}/chapters/${chapterId}/comic`,
            {
                credentials: "include",
                method: "GET",
            },
        );

        const status = response.status;
        if (!response.ok) {
            const text = await response.text();
            return [undefined, { error: text, status }];
        }

        const data = await response.json();
        return [data, undefined];
    } catch (error) {
        return [undefined, { error, status: 500 }];
    }
}
