import { Result } from "@/utils/err";
import { toast } from "sonner";
import { fetchApi } from "@/utils/api";
import { getSeriesInfo } from "./series";

export interface ChapterPricing {
    seriesId: number;
    volumeId: number;
    chapterId: number;
    price: number | null;
}

export async function setChapterPricing(data: ChapterPricing): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${data.seriesId}/volumes/${data.volumeId}/chapters/${data.chapterId}/price`,
        { ...data },
    );
}

export interface ChapterDetails {
    chapterId: number;
    chapterNumber: number;
    versionId: string;
    versionName: string;
    title: string;
    thumbnail: string;
    content: ChapterContent[];
    note: string;
}

export async function getChapterVersionPreview(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    versionId: string,
): Promise<Result<ChapterDetails>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/comic/versions/${versionId}/preview`,
    );
}

export async function editChapterVersionSummary(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    versionId: string,
    versionName: string,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/versions/${versionId}`,
        {
            seriesId,
            volumeId,
            chapterId,
            versionId,
            versionName,
        },
    );
}

export async function deleteChapterVersion(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    versionId: string,
): Promise<Result<void>> {
    return await fetchApi(
        "DELETE",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/versions/${versionId}`,
    );
}

export async function restoreChapterVersion(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    versionId: string,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/restore-version`,
        {
            seriesId,
            volumeId,
            chapterId,
            versionId,
        },
    );
}

export interface ChapterContent {
    imageUrl: string;
    imageName: string;
    imageSize: number;
    imageWidth: number;
    imageHeight: number;
}

export interface ChapterVersion {
    versionId: string;
    versionName: string;
    isCurrent: boolean;
    isPublished: boolean;
    isAutoSave: boolean;
    title: string;
    thumbnail: string;
    content: ChapterContent[] | string;
    note: string;
    timestamp: string;
}

export interface ChapterVersionResponse {
    items: ChapterVersion[];
    pageNumber: number;
    totalPages: number;
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

export async function getChapterVersions(
    isComic: boolean,
    seriesId: string,
    volumeId: string,
    chapterId: string,
    versionName: string,
    includeAutoSave: boolean,
    pageNumber: number,
    pageSize: number,
    from?: Date,
    to?: Date,
): Promise<Result<ChapterVersionResponse>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/${isComic ? "comic" : "novel"}/versions`,
        undefined,
        {
            params: {
                versionName,
                includeAutoSave,
                pageNumber,
                pageSize,
                to: to?.toISOString(),
                from: from?.toISOString(),
            },
        },
    );
}

export async function reorderChapter(seriesId: string, volumeId: string, chapterId: string, previousChapterId: string) {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/reorder`,
        {
            seriesId: parseInt(seriesId),
            volumeId: parseInt(volumeId),
            chapterId: parseInt(chapterId),
            previousChapterId: parseInt(previousChapterId),
        },
    );
}

export async function publishChapter(seriesId: string, volumeId: string, chapterId: string, date: Date | null) {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/publish`,
        {
            seriesId,
            volumeId,
            chapterId,
            publishDate: date,
        },
    );
}

export async function unpublishChapter(seriesId: string, volumeId: string, chapterId: string) {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/unpublish`,
    );
}

export async function getChapterContentNovel(
    seriesId: string,
    volumeId: string,
    chapterId: string,
): Promise<Result<any>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/novel`,
    );
}

export async function getChapterContentComic(
    seriesId: string,
    volumeId: string,
    chapterId: string,
): Promise<Result<any>> {
    return await fetchApi(
        "GET",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/comic`,
    );
}

export async function createChapterNovel(seriesId: string, volumeId: string): Promise<Result<string>> {
    return await fetchApi("POST", `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/novel`, {
        seriesId,
        volumeId,
        title: "Chương chưa có tiêu đề",
        content: "<p>Chèn nội dung chương vào đây!</p>",
        note: "",
    });
}

export async function createChapterComic(seriesId: string, volumeId: string): Promise<Result<string>> {
    return await fetchApi("POST", `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/comic`, {
        seriesId,
        volumeId,
        title: "Chương chưa có tiêu đề",
        content: [],
        note: "",
    });
}

export async function createChapter(seriesId: string, volumeId: string): Promise<Result<string>> {
    const [series, err] = await getSeriesInfo(seriesId);
    if (err) {
        return [undefined, err];
    }

    if (series.type.toString() === "1") {
        return await createChapterNovel(seriesId, volumeId);
    } else {
        return await createChapterComic(seriesId, volumeId);
    }
}

export async function editChapterNovel(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    chapterDetails: any,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/novel`,
        {
            ...chapterDetails,
            seriesId,
            volumeId,
            chapterId,
        },
    );
}

export async function editChapterComic(
    seriesId: string,
    volumeId: string,
    chapterId: string,
    chapterDetails: any,
): Promise<Result<void>> {
    return await fetchApi(
        "PUT",
        `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}/comic`,
        {
            ...chapterDetails,
            seriesId,
            volumeId,
            chapterId,
        },
    );
}

export async function removeChapter(seriesId: string, volumeId: string, chapterId: string): Promise<Result<void>> {
    return await fetchApi("DELETE", `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/${chapterId}`);
}

export async function bulkPublishChapter(seriesId: string, volumeId: string, chapterIds: string[]) {
    return await fetchApi("PUT", `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/bulk-publish`, {
        seriesId,
        volumeId,
        chapterIds: chapterIds.map((x) => parseInt(x)),
    });
}

export async function bulkUnpublishChapter(seriesId: string, volumeId: string, chapterIds: string[]) {
    await Promise.all(chapterIds.map(async (x) => await unpublishChapter(seriesId, volumeId, x)));

    return [undefined, undefined];
}

export async function bulkRemoveChapter(seriesId: string, volumeId: string, chapterIds: string[]) {
    return await fetchApi("PUT", `/api/AuthorStudio/series/${seriesId}/volumes/${volumeId}/chapters/bulk-delete`, {
        id: seriesId,
        volumeId,
        chapterIds: chapterIds.map((x) => parseInt(x)),
    });
}

export interface UploadChapterImageResp {
    fileName: string;
}

export async function uploadChapterImage(file: File): Promise<Result<UploadChapterImageResp>> {
    if (!(await validateChapterImage(file))) {
        return [undefined, "Lỗi khi xác thực"];
    }

    const formData = new FormData();
    formData.append("file", file);

    return await fetchApi("POST", `/api/AuthorStudio/series/chapters`, formData);
}

export function validateChapterImage(fi: File): Promise<boolean> {
    return new Promise((resolve, reject) => {
        const validTypes = ["image/jpeg", "image/png", "image/gif"];
        if (!validTypes.includes(fi.type)) {
            toast.error("Tải lên không thành công", {
                description: "Vui lòng tải lên tệp hình ảnh hợp lệ",
            });
            resolve(false);
        }

        if (fi.size > 5 * 1024 * 1024) {
            toast.error("Tải lên không thành công", {
                description: "Vui lòng tải lên hình ảnh có kích thước nhỏ hơn 5MB",
            });
            resolve(false);
        }

        resolve(true);
    });
}
