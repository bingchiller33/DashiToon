import { promiseErr, Result } from "@/utils/err";
import { fetchApi } from "..";

export interface CategoryRating {
    category: string;
    option: number;
}

export interface SeriesInfo {
    id: number;
    title: string;
    author: string;
    status: number;
    synopsis: string;
    thumbnail: string;
    type: number;
    genres: string[];
    contentRating: string;
    categoryRatings: CategoryRating[];
}

export async function getSeriesInfo(seriesId: string): Promise<Result<SeriesInfo>> {
    return await fetchApi("GET", `/api/AuthorStudio/series/${seriesId}`);
}

export interface APISeriesResponse {
    id: number;
    title: string;
    status: number;
    thumbnail: string;
    type: number;
    genres: string[];
    contentRating: number;
    updatedAt: string;
}

export async function getSeries(): Promise<Result<APISeriesResponse[]>> {
    return await fetchApi("GET", `/api/AuthorStudio/series`);
}
