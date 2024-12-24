import { Result } from "@/utils/err";
import { fetchApi } from "..";

export interface VolumeInfoResponse {
    volumeId: number;
    volumeNumber: number;
    name: string;
    introduction: string;
    chapterCount: number;
}

export async function getVolumeInfo(
    seriesId: string,
    volumeId: string,
): Promise<Result<VolumeInfoResponse>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}`,
    );
}

export interface ChapterItem {
    id: string;
    chapterNumber: number;
    publishedDate: string | null;
    title: string;
    thumbnail: string;
    status: number;
    updatedAt: string;
}

export interface GetChapterListResponse {
    items: ChapterItem[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getChapters(
    seriesId: string,
    volumeId: string,
    pageNumber: number = 1,
    pageSize: number = 10,
    title?: string,
    status?: string,
): Promise<Result<GetChapterListResponse>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters`,
        undefined,
        {
            params: {
                title: title === "" ? undefined : title,
                status,
                pageNumber,
                pageSize,
            },
        },
    );
}

export async function getPrevChapter(
    seriesId: string,
    volumeId: string,
    chapterId: string,
): Promise<Result<ChapterItem | undefined>> {
    const [data, err] = await getChaptersAll(seriesId, volumeId);
    if (err) {
        return [undefined, err];
    }

    const curChap = data.find((x) => x.id.toString() === chapterId);
    if (!curChap) {
        return [undefined, chapterId];
    }

    const prev = data
        .toSorted((a, b) => b.chapterNumber - a.chapterNumber)
        .find((x) => x.chapterNumber < curChap.chapterNumber);

    return [prev, undefined];
}

export async function getChaptersAll(
    seriesId: string,
    volumeId: string,
): Promise<Result<GetChapterListResponse["items"]>> {
    const [search, errS] = await getChapters(seriesId, volumeId);
    if (errS) {
        return [undefined, errS];
    }

    const [data, err] = await getChapters(
        seriesId,
        volumeId,
        1,
        search.totalCount,
    );
    if (err) {
        return [undefined, err];
    }

    return [data.items, undefined];
}

export interface UpdateVolumeInfoRequest {
    name: string;
    introduction?: string;
}

export async function updateVolumeInfo(
    seriesId: string,
    volumeId: string,
    data: UpdateVolumeInfoRequest,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}`,
        { ...data, seriesId, volumeId },
    );
}

export async function removeVolume(
    seriesId: string,
    volumeId: string,
): Promise<Result<void>> {
    return await fetchApi(
        "DELETE",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}`,
    );
}
