import * as env from "@/utils/env";
import { promiseErr, Result } from "@/utils/err";
import { toast } from "sonner";
import { fetchApi } from ".";

export interface UploadThumbnailResp {
    fileName: string;
}

export async function uploadThumbnail(
    file: File,
): Promise<Result<UploadThumbnailResp>> {
    if (!(await validateThumbnail(file))) {
        return [undefined, "Lỗi khi xác thực"];
    }

    const formData = new FormData();
    formData.append("file", file);

    const [response, err] = await promiseErr(
        fetch(`${env.getBackendHost()}/api/AuthorStudio/series/thumbnails`, {
            method: "POST",
            body: formData,
            credentials: "include",
        }),
    );

    if (err) {
        console.error("Không thể tải lên hình thu nhỏ", err);
        toast.error("Tải lên thất bại", {});
        return [undefined, err];
    }

    if (!response.ok) {
        console.error("Không thể tải lên hình thu nhỏ", await response.text());
        toast.error("Tải lên thất bại", {});
        return [undefined, response];
    }

    const thumbnailData = await response.json();
    toast.success("Tải lên hình thu nhỏ thành công", {});
    return [thumbnailData, undefined];
}

export function validateThumbnail(fi: File): Promise<boolean> {
    return new Promise((resolve, reject) => {
        const validTypes = ["image/jpeg", "image/png", "image/gif"];
        if (!validTypes.includes(fi.type)) {
            toast.error("Tải lên thất bại", {
                description: "Vui lòng tải lên tệp hình ảnh hợp lệ",
            });
            resolve(false);
        }

        if (fi.size > 1024 * 1024) {
            toast.error("Tải lên thất bại", {
                description: "Vui lòng tải lên hình ảnh nhỏ hơn 1MB",
            });
            resolve(false);
        }

        const img = new window.Image();
        img.onload = () => {
            const aspectRatio = img.width / img.height;
            console.log(aspectRatio, img.width, img.height);
            if (Math.abs(aspectRatio - 3 / 4) > 0.01) {
                toast.error("Tải lên thất bại", {
                    description:
                        "Vui lòng tải lên hình ảnh có tỷ lệ khung hình 3:4",
                });
                resolve(false);
            }

            resolve(true);
        };
        img.onerror = () => {
            toast.error("Tải lên thất bại", {
                description:
                    "Vui lòng tải lên hình ảnh có tỷ lệ khung hình 3:4",
            });
            resolve(false);
        };
        img.src = URL.createObjectURL(fi);
    });
}

export interface GetImageResp {
    imageHeight: number;
    imageName: string;
    imageSize: number;
    imageUrl: string;
    imageWidth: number;
}

export async function getImage(
    name: string,
    type: string,
): Promise<Result<GetImageResp>> {
    return await fetchApi("GET", `/api/Images/${name}`, undefined, {
        params: { type },
    });
}
